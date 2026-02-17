using PolicyEventHub.Applications.Domain.Abstractions;
using PolicyEventHub.Applications.DTOs;

namespace PolicyEventHub.Infrastructure.Services
{
    public class OSAGOProxyService : IOSAGOProxyService
    {
        private readonly ILogger<OSAGOProxyService> _logger;
        private readonly ILegacyOSAGODataRetriver _legacyOSAGODataRetriver;

        #region OSAGOService Constructur
        public OSAGOProxyService(ILogger<OSAGOProxyService> logger,
          ILegacyOSAGODataRetriver legacyOSAGODataRetriver)
        {
            _logger = logger;
            _legacyOSAGODataRetriver = legacyOSAGODataRetriver;
        }
        #endregion
        #region GetFilteredCancelledCompulsoryPoliciesAsync
        public async Task<PagedResultDto<OSAGOResponseDto>> GetFilteredCancelledCompulsoryPoliciesAsync(OSAGORequestDto requestDto, CancellationToken ct)
        {
            _logger.LogInformation("Method {MethodName} started to work with the request: {@Request}", nameof(GetFilteredCancelledCompulsoryPoliciesAsync), requestDto);
            try
            {
                var result = await _legacyOSAGODataRetriver.GetFilteredCancelledCompulsoryPoliciesAsync(requestDto, ct);
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting filtered cancelled compulsory policies");
                throw;
            }
        }
        #endregion

        #region GetCancelledCompulsoryPolicyById
        public async Task<OSAGOResponseDto> GetCancelledCompulsoryPolicyByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _legacyOSAGODataRetriver.GetCancelledCompulsoryPolicyByIdAsync(id, cancellationToken);
        }
        #endregion


        #region GetIframeUrlUnregisteredAsync
        public async Task<string> GetIframeUrlUnregisteredAsync(
        int id,
        CancellationToken cancellationToken)
        {
            _logger.LogInformation("Method {MethodName} started with request: {@id}", nameof(GetIframeUrlUnregisteredAsync), id);

            var result = await _legacyOSAGODataRetriver.GetIframeUrlUnregisteredAsync(id, cancellationToken);
            return result;
        }
        #endregion

        #region UpdateCancelledCompulsoryPolicyAsync
        public async Task UpdateCancelledCompulsoryPolicyAsync(int id, CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Method {MethodName} started with request: {@id}", nameof(UpdateCancelledCompulsoryPolicyAsync), id);

            await _legacyOSAGODataRetriver.UpdateCancelledCompulsoryPolicyAsync(id, cancelledCompulsoryPolicyUpdateDto, cancellationToken);
        }
        #endregion

     
    }
}