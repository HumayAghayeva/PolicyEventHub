using Dapper;
using DataBridge.API.Abstraction;
using DataBridge.API.DTO;
using DataBridge.API.Enum;
using DataBridge.API.Query;
using System.Data;

namespace DataBridge.API.Services
{

    public class CompulsoryCanceledPolicyService : ICancelledCompulsaryPolicyService
    {
        private readonly IRawSqlExecutorFactory _sqlExecutorFactory;
        private readonly ILogger<CompulsoryCanceledPolicyService> _logger;
        public CompulsoryCanceledPolicyService(ILogger<CompulsoryCanceledPolicyService> logger,
           IRawSqlExecutorFactory sqlExecutorFactory)
        {
            _logger = logger;
            _sqlExecutorFactory = sqlExecutorFactory;
        }
        public async Task<ApiResponse<CancelledCompulsoryPolicyResponseDto>> GetCancelledCompulsoryPolicyByIDAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{method} : id: {id}", nameof(GetCancelledCompulsoryPolicyByIDAsync), id);

            var rawSqlExecutor = _sqlExecutorFactory.Create(DatabaseName.CIBM);

            var parameters = new DynamicParameters();
            parameters.Add("id", id, DbType.Int32);

            var data = await rawSqlExecutor.FetchSingleAsync<CancelledCompulsoryPolicyResponseDto>(new QueryDefinition(CompulsoryCancelledQuery.GetCancelledCompulsoryPolicyById, parameters), cancellationToken);

            return new ApiResponse<CancelledCompulsoryPolicyResponseDto>(data);
        }

        public async Task<string> GetUserPincodeByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{method} : userId: {userId}", nameof(GetUserPincodeByUserIdAsync), userId);

            var rawSqlExecutor = _sqlExecutorFactory.Create(DatabaseName.CIBM);

            var parameters = new DynamicParameters();
            parameters.Add("userId", userId, DbType.String);

            var data = await rawSqlExecutor.FetchSingleAsync<string>(new QueryDefinition(CompulsoryCancelledQuery.GetUserPincodeByUserId, parameters), cancellationToken);

            return data;
        }
        public async Task<ApiResponse<IEnumerable<CancelledCompulsoryPolicyResponseDto>>> GetCancelledCompulsoryPoliciesAsync(CompulsoryCancelledPolicyRequestDto requestDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Method}: Starting. request: {request}", nameof(GetCancelledCompulsoryPoliciesAsync), requestDto);

            string? pinCode = await GetUserPincodeByUserIdAsync(requestDto.UserId, cancellationToken);

            var rawSqlExecutor = _sqlExecutorFactory.Create(DatabaseName.CIBM);

            var parameters = new DynamicParameters();
            parameters.Add("startDate", requestDto.StartDate, DbType.DateTime);
            parameters.Add("endDate", requestDto.EndDate, DbType.DateTime);
            parameters.Add("plate", requestDto.Plate, DbType.String);
            parameters.Add("pin", requestDto.Pin, DbType.String);
            parameters.Add("insuredFullname", requestDto.InsuredFullName, DbType.String);
            parameters.Add("certificationNumber", requestDto.CertificationNumber, DbType.String);
            parameters.Add("operatorPinCode", pinCode, DbType.String);
            parameters.Add("pageSize", requestDto.PageSize, DbType.Int32);
            parameters.Add("pageNumber", requestDto.PageNumber, DbType.Int32);

            var data = await rawSqlExecutor.FetchAsync<CancelledCompulsoryPolicyResponseDto>(new QueryDefinition(
            CompulsoryCancelledQuery.GetCancelledCompulsoryPolicies, parameters));

            return new ApiResponse<IEnumerable<CancelledCompulsoryPolicyResponseDto>>(data);
        }

        public async Task<int> GetCancelledCompulsoryPoliciesCountAsync(CompulsoryCancelledPolicyCountRequestDto requestDto,
CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Method} started. Request: {@Request}", nameof(GetCancelledCompulsoryPoliciesCountAsync),
                requestDto);

            string? pinCode = await GetUserPincodeByUserIdAsync(requestDto.UserId, cancellationToken);

            var rawSqlExecutor = _sqlExecutorFactory.Create(DatabaseName.CIBM);

            var parameters = new DynamicParameters();
            parameters.Add("startDate", requestDto.StartDate, DbType.DateTime);
            parameters.Add("endDate", requestDto.EndDate, DbType.DateTime);
            parameters.Add("operatorPinCode", pinCode, DbType.String);
            parameters.Add("plate", requestDto.Plate, DbType.String);
            parameters.Add("pin", requestDto.Pin, DbType.String);
            parameters.Add("certificationNumber", requestDto.CertificationNumber, DbType.String);
            parameters.Add("insuredFullname", requestDto.InsuredFullName, DbType.String);

            try
            {
                var count = (await rawSqlExecutor.FetchAsync<int>(
                new QueryDefinition(
                    CompulsoryCancelledQuery.GetCancelledCompulsoryPoliciesCount,
                    parameters),
                cancellationToken)).FirstOrDefault();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting cancelled compulsory policies count");
                throw;
            }
        }

        public async Task<ApiResponse> UpdateCancelledCompulsoryPolicyAsync(int id, CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Method} started. updateRequest: {@updateRequest} and id:{@id}",
                nameof(UpdateCancelledCompulsoryPolicyAsync),
                cancelledCompulsoryPolicyUpdateDto, id);

            var rawSqlExecutor = _sqlExecutorFactory.Create(DatabaseName.CIBM);

            var parameters = new DynamicParameters();
            parameters.Add("id", id, DbType.Int32);
            parameters.Add("insuredFullName", cancelledCompulsoryPolicyUpdateDto.InsuredFullName, DbType.String);
            parameters.Add("pin", cancelledCompulsoryPolicyUpdateDto.PIN, DbType.String);
            parameters.Add("phoneNumber", cancelledCompulsoryPolicyUpdateDto.PhoneNumber, DbType.String);
            parameters.Add("email", cancelledCompulsoryPolicyUpdateDto.Email, DbType.String);
            parameters.Add("plate", cancelledCompulsoryPolicyUpdateDto.Plate, DbType.String);
            parameters.Add("driverLicense", cancelledCompulsoryPolicyUpdateDto.DriverLicense, DbType.String);
            parameters.Add("docNumber", cancelledCompulsoryPolicyUpdateDto.DocNumber, DbType.String);

            try
            {
                await rawSqlExecutor.ExecuteAsync(new QueryDefinition(CompulsoryCancelledQuery.UpdateCompulsaryPolicy,
                parameters, CommandType.StoredProcedure));
                return ApiResponse.Success("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while update cancelled compulsory policy by Id: {@id}", id);
                throw;
            }
        }
    }
}