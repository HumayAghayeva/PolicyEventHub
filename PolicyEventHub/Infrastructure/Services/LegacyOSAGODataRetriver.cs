using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PolicyEventHub.Applications.Domain.Abstractions;
using PolicyEventHub.Applications.DTOs;
using PolicyEventHub.Applications.Externals;
using PolicyEventHub.Applications.Observability;
using PolicyEventHub.Infrastructure.Configurations;
using PolicyEventHub.Models.Api;
using Polly;
using Polly.Retry;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace PolicyEventHub.Infrastructure.Services
{
    public class LegacyOSAGODataRetriver : ILegacyOSAGODataRetriver
    {
        private readonly ILogger<LegacyOSAGODataRetriver> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private string _jwtToken = string.Empty;
        private readonly GatewayOptions _gatewayOptions;
        private readonly IJWTTokenProvider _tokenProvider;
        private readonly InsureIframeServiceSettings _insureIframeSettingOptions;
        private readonly ICompulsoryMotorSaleValidator _compulsoryMotorSaleDtoValidator;
        private readonly CompulsoryMotorSaleSettings _compulsoryMotorSaleSettings;

        private readonly IAppTracer _tracer;
        private readonly IAppMetrics _metrics;
        public LegacyOSAGODataRetriver(ILogger<LegacyOSAGODataRetriver> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<HttpRetryOptions> retryOptions,
            IOptions<GatewayOptions> gatewayOptions,
            IJWTTokenProvider tokenProvider,
            IOptions<InsureIframeServiceSettings> insureIframeSettingOptions,
            IOptions<CompulsoryMotorSaleSettings> compulsoryMotorSaleSettings,
             ICompulsoryMotorSaleValidator compulsoryMotorSaleDtoValidator,
          IAppTracer tracer,
            IAppMetrics metrics)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _gatewayOptions = gatewayOptions.Value;
            _tokenProvider = tokenProvider;
            _compulsoryMotorSaleSettings = compulsoryMotorSaleSettings.Value;
            _insureIframeSettingOptions = insureIframeSettingOptions.Value;
            _compulsoryMotorSaleDtoValidator = compulsoryMotorSaleDtoValidator;
            var retry = retryOptions.Value;
            _tracer = tracer;
            _metrics = metrics;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: retry.RetryCount,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(retry.BaseDelaySeconds * Math.Pow(2, attempt - 1)),
                    onRetry: (outcome, delay, retryAttempt, ctx) =>
                    {
                        _logger.LogWarning(
                            "OSAGOService retry {RetryAttempt}. Delay {Delay}s. Reason: {Reason}",
                            retryAttempt,
                            delay.TotalSeconds,
                            outcome.Exception?.Message ??
                            outcome.Result?.StatusCode.ToString() ??
                            "Unknown");
                    });

        }
        #region GetFilteredCancelledCompulsoryPoliciesAsync
        public async Task<PagedResultDto<OSAGOResponseDto>> GetFilteredCancelledCompulsoryPoliciesAsync(OSAGORequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Method {MethodName} started. Request: {@Request}",
                nameof(GetFilteredCancelledCompulsoryPoliciesAsync),
                request);

            const string cancelledCompulsoryPoliciesEndpoint =
                "api/v1/CancelledCompulsoryPolicies";

            using var span = _tracer.StartSpan("CancelledCompulsoryPolicies.GetFilteredCancelledCompulsoryPoliciesAsync",
                        new Dictionary<string, object?>
                        {
                            ["request.startDate"] = request.StartDate.ToString("yyyy-MM-dd"),
                            ["request.endDate"] = request.EndDate.ToString("yyyy-MM-dd"),
                            ["request.pin"] = request.PIN,
                            ["request.plate"] = request.Plate,
                            ["request.certificationNumber"] = request.CertificationNumber,
                            ["request.insuredFullName"] = request.InsuredFullName,
                            ["request.page"] = request.Page,
                            ["request.pageSize"] = request.PageSize
                        });
            using var timer = _metrics.MeasureUseCase("Allowance.Check");

            try
            {

                var loginResponse = await RetrieveAuthAsync(request.SessionId).ConfigureAwait(false);

                var client = _httpClientFactory.CreateClient("InsureIframeServiceSettings");
                client.BaseAddress = new Uri(_insureIframeSettingOptions.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                client.DefaultRequestHeaders.Add(_gatewayOptions.GatewayKey, _gatewayOptions.GatewayValue);

                request.UserId = loginResponse.UserId;

                var query = new Dictionary<string, string?>
                {
                    ["startDate"] = request.StartDate.ToString("yyyy-MM-dd"),
                    ["endDate"] = request.EndDate.ToString("yyyy-MM-dd"),
                    ["pageNumber"] = request.Page.ToString(),
                    ["pageSize"] = request.PageSize.ToString(),
                    ["pin"] = string.IsNullOrWhiteSpace(request.PIN) ? null : request.PIN,
                    ["plate"] = string.IsNullOrWhiteSpace(request.Plate) ? null : request.Plate,
                    ["insuredFullname"] = string.IsNullOrWhiteSpace(request.InsuredFullName) ? null : request.InsuredFullName,
                    ["certificationNumber"] = string.IsNullOrWhiteSpace(request.CertificationNumber) ? null : request.CertificationNumber,
                    ["userId"] = loginResponse.UserId.ToString()
                };

                var dataUrl = QueryHelpers.AddQueryString(
                    new Uri(client.BaseAddress!, cancelledCompulsoryPoliciesEndpoint).ToString(),
                    query);

                _logger.LogInformation(
                    "Sending DATA request to InsureIframe API. Endpoint: {Endpoint}",
                    dataUrl);

                var dataResponse = await _retryPolicy.ExecuteAsync(
                    () => client.GetAsync(dataUrl, cancellationToken));

                var dataContent = await dataResponse.Content
                    .ReadAsStringAsync(cancellationToken);

                if (!dataResponse.IsSuccessStatusCode)
                    throw new ApplicationException(
                        $"DATA request failed. StatusCode={dataResponse.StatusCode}, Response={dataContent}");

                var dataResult =
                    JsonConvert.DeserializeObject<ApiResponse<IEnumerable<OSAGOResponseDto>>>(dataContent)
                    ?? throw new ApplicationException("DATA response is null");

                //Count request
                var totalCount = await GetCancelledCompulsoryPoliciesCountAsync(
                    request,
                    loginResponse.Token,
                    client,
                    cancellationToken);

                return new PagedResultDto<OSAGOResponseDto>
                {
                    Items = dataResult.Data,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _metrics.IncrementBusinessError("GetFilteredCancelledCompulsoryPoliciesAsync_failed");
                _logger.LogError(ex, "Error while getting filtered cancelled compulsory policies");

                throw;
            }
        }
        #endregion

        #region GetCancelledCompulsoryPoliciesCountAsync
        private async Task<int> GetCancelledCompulsoryPoliciesCountAsync(
            OSAGORequestDto request,
            string token,
            HttpClient client,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "{Method} started. Request: {@Request}",
                nameof(GetCancelledCompulsoryPoliciesCountAsync),
                request);

            const string countEndpoint =
                "api/v1/CancelledCompulsoryPolicies/count";

            var query = new Dictionary<string, string?>
            {
                ["startDate"] = request.StartDate.ToString("yyyy-MM-dd"),
                ["endDate"] = request.EndDate.ToString("yyyy-MM-dd"),
                ["pin"] = string.IsNullOrWhiteSpace(request.PIN) ? null : request.PIN,
                ["plate"] = string.IsNullOrWhiteSpace(request.Plate) ? null : request.Plate,
                ["certificationNumber"] = string.IsNullOrWhiteSpace(request.CertificationNumber) ? null : request.CertificationNumber,
                ["insuredFullname"] = string.IsNullOrWhiteSpace(request.InsuredFullName) ? null : request.InsuredFullName,
                ["userId"] = request.UserId.ToString()
            };

            var countUrl = QueryHelpers.AddQueryString(
                new Uri(client.BaseAddress!, countEndpoint).ToString(),
                query);

            _logger.LogInformation(
                "Sending COUNT request to InsureIframe API. Endpoint: {Endpoint}",
                countUrl);

            var response = await _retryPolicy.ExecuteAsync(
                () => client.GetAsync(countUrl, cancellationToken));

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(
                    $"COUNT request failed. StatusCode={response.StatusCode}, Response={content}");

            var result = JsonConvert.DeserializeObject<ApiResponse<int>>(content)
                ?? throw new ApplicationException("COUNT response is null");

            return result.Data;
        }

        #endregion

        #region  GetCancelledCompulsoryPolicyByIdAsync
        public async Task<OSAGOResponseDto> GetCancelledCompulsoryPolicyByIdAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Method {MethodName} started to work with the request: {Id}", nameof(GetCancelledCompulsoryPolicyByIdAsync), id);

            #region Add metrics
            using var span = _tracer.StartSpan("CancelledCompulsoryPolicies.GetCancelledCompulsoryPolicyByIdAsync",
                  new Dictionary<string, object?>
                  {
                      ["id"] = id
                  });
            using var timer = _metrics.MeasureUseCase("GetCancelledCompulsoryPolicyByIdAsync.Check");
            #endregion

            const string cancelledCompulsoryPolicies = "api/v1/CancelledCompulsoryPolicies";

            try
            {
                var token = await RetrieveTokenAsync().ConfigureAwait(false);

                var client = _httpClientFactory.CreateClient("InsureIframeServiceSettings");
                client.BaseAddress = new Uri(_insureIframeSettingOptions.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add(_gatewayOptions.GatewayKey, _gatewayOptions.GatewayValue);

                var finalUrl = new Uri(client.BaseAddress!, $"{cancelledCompulsoryPolicies}/{id}");

                // Execute request with retry
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await client.GetAsync(finalUrl).ConfigureAwait(false);
                }).ConfigureAwait(false);

                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Request to Insure Iframe API failed. StatusCode: {StatusCode}, Endpoint: {Endpoint}, Response: {Response}", response.StatusCode, finalUrl, responseText);

                    throw new ApplicationException(
                        $"Request failed with status code {response.StatusCode}. Response: {responseText}");
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<OSAGOResponseDto>>(responseText);

                if (result == null)
                    throw new ApplicationException($"Response from InsureIframe API. Endpoint: {finalUrl}");

                return result.Data;
            }
            catch (Exception ex)
            {
                _metrics.IncrementBusinessError("GetCancelledCompulsoryPolicyByIdAsync_failed");
                _logger.LogError(ex, "Error while getting compulsory policy by id");

                throw;
            }

        }
        #endregion
        #region GetIframeUrlUnregisteredAsync
        public async Task<string> GetIframeUrlUnregisteredAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Method {MethodName} started with request: {@id}",
                nameof(GetIframeUrlUnregisteredAsync), id);

            using var span = _tracer.StartSpan("CancelledCompulsoryPolicies.GetFilteredCancelledCompulsoryPoliciesAsync",
                      new Dictionary<string, object?> { ["id"] = id });

            using var timer = _metrics.MeasureUseCase("GetIframeUrlUnregisteredAsync.Check");

            var client = _httpClientFactory.CreateClient("CompulsoryMotorSaleSettings");
            client.BaseAddress = new Uri(_compulsoryMotorSaleSettings.BaseUrl);

            // Basic Auth
            var credentials = $"{_compulsoryMotorSaleSettings.Username}:{_compulsoryMotorSaleSettings.Password}";
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add(_gatewayOptions.GatewayKey, _gatewayOptions.GatewayValue);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authValue);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var apiUrl = new Uri(client.BaseAddress!,"CompulsoryMotor/getIframeUrl");

            // Load policy data
            var osagoResponse = await GetCancelledCompulsoryPolicyByIdAsync(id, cancellationToken);

            // Prepare request DTO
            var compulsoryMotorSaleRequest = new CompulsoryMotorSaleDto
            {
                Phone = osagoResponse.PhoneNumber,
                Email = osagoResponse.Email,
                Plate = osagoResponse.Plate,
                Serial = osagoResponse.CertificationNumber,
                IdNumber = osagoResponse.DocNumber,
                IsWeb = true,
                LicenseId = true,
                LicenseNumber = string.IsNullOrEmpty(osagoResponse.DriverLicense) ? string.Empty : osagoResponse.DriverLicense.Substring(2, 6),
                LicenseSerial = string.IsNullOrEmpty(osagoResponse.DriverLicense) ? string.Empty : osagoResponse.DriverLicense.Substring(0, 2),
                Pin = osagoResponse.PIN
            };

            // Validate DTO
            await _compulsoryMotorSaleDtoValidator.ValidateSendAsync(compulsoryMotorSaleRequest, cancellationToken);

            // Perform external POST
            HttpResponseMessage response = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var content = new StringContent(
                    JsonConvert.SerializeObject(compulsoryMotorSaleRequest),
                    Encoding.UTF8,
                    "application/json");

                return await client.PostAsync(apiUrl, content, cancellationToken);
            });

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request to API: {ApiUrl} failed. StatusCode: {StatusCode}, Response: {Response}",
                    apiUrl, response.StatusCode, responseBody);

                throw new ApplicationException(
                    $"Request to API '{apiUrl}' failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
            }

            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(responseBody);

            if (result?.Data == null)
            {
                throw new ApplicationException("API returned null iframe URL");
            }
            return result.Data;
        }


        #endregion

        #region UpdateCancelledCompulsoryPolicyAsync
        public async Task UpdateCancelledCompulsoryPolicyAsync(
         int id,
         CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto,
         CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Method {MethodName} started. updateRequest: {@updateRequest}, id: {Id}",
                nameof(UpdateCancelledCompulsoryPolicyAsync),
                cancelledCompulsoryPolicyUpdateDto,
                id);

            var token = await RetrieveTokenAsync().ConfigureAwait(false);

            const string updateCancelledCompulsoryPolicyEndpoint = "api/v1/CancelledCompulsoryPolicies";

            var client = _httpClientFactory.CreateClient("InsureIframeServiceSettings");
            client.BaseAddress = new Uri(_insureIframeSettingOptions.BaseUrl);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add(_gatewayOptions.GatewayKey, _gatewayOptions.GatewayValue);

            var finalUrl = new Uri(client.BaseAddress!, $"{updateCancelledCompulsoryPolicyEndpoint}?id={id}");

            HttpResponseMessage response = await _retryPolicy.ExecuteAsync(async () =>
            {

                var content = new StringContent(
                    JsonConvert.SerializeObject(cancelledCompulsoryPolicyUpdateDto),
                    Encoding.UTF8,
                    "application/json");

                return await client.PutAsync(finalUrl, content, cancellationToken);
            });

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Request to API {FinalUrl} failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
                    finalUrl,
                    response.StatusCode,
                    responseBody);

                throw new ApplicationException(
                    $"Request to API '{finalUrl}' failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
            }

            _logger.LogInformation(
                "CancelledCompulsoryPolicy with id {Id} updated successfully",
                id);
        }


        #endregion


        #region RetrieveTokenAsync
        private async Task<string> RetrieveTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_jwtToken) && !IsTokenExpired(_jwtToken))
                return _jwtToken;

            var client = _httpClientFactory.CreateClient("InsureIframeServiceSettings");
            client.BaseAddress = new Uri(_insureIframeSettingOptions.BaseUrl);
            client.DefaultRequestHeaders.Add(_gatewayOptions.GatewayKey, _gatewayOptions.GatewayValue);

            var loginPayload = new
            {
                _insureIframeSettingOptions.Username,
                //Password = LegacyCryptography.TripleDecrypt(_insureIframeSettingOptions.Password, _insureIframeSettingOptions.EncryptionKey)
            };

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var content = new StringContent(
                    JsonConvert.SerializeObject(loginPayload),
                Encoding.UTF8,
                "application/json");

                return await client.PostAsync(_insureIframeSettingOptions.AuthPath, content).ConfigureAwait(false);
            }).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Auth failed. Status: {Status}, Body: {Body}",
                    response.StatusCode, json);

                throw new ApplicationException("Service authentication failed!");
            }

            var authResponse = JsonConvert.DeserializeObject<ApiResponse<AuthUserDto>>(json);

            _jwtToken = authResponse?.Data?.Token
                        ?? throw new ApplicationException("Auth response did not contain token!");

            return _jwtToken;
        }
        public async Task<LoginResponse> RetrieveAuthAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving JWT token for session: {SessionId}", sessionId);

                return await _tokenProvider.RetrieveTokenAsync(sessionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token from Insure Service");
                throw;
            }
        }

        #endregion
        private bool IsTokenExpired(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                return jwt.ValidTo < DateTime.UtcNow.AddMinutes(5);
            }
            catch
            {
                return true;
            }
        }
    }
}