using PolicyEventHub.Applications.DTOs;

namespace PolicyEventHub.Applications.Domain.Abstractions
{
    public interface IOSAGOProxyService
    {
        Task<PagedResultDto<OSAGOResponseDto>> GetFilteredCancelledCompulsoryPoliciesAsync(
            OSAGORequestDto request,
            CancellationToken cancellationToken);

        Task<OSAGOResponseDto> GetCancelledCompulsoryPolicyByIdAsync(int id, CancellationToken cancellationToken);

        Task<string> GetIframeUrlUnregisteredAsync(int id, CancellationToken cancellationToken);
        Task UpdateCancelledCompulsoryPolicyAsync(int id, CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto, CancellationToken cancellationToken);
   
    }
}
