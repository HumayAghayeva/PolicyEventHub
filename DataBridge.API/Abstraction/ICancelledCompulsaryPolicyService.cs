using DataBridge.API.DTO;

namespace DataBridge.API.Abstraction
{
    public interface ICancelledCompulsaryPolicyService
    {
            Task<ApiResponse<IEnumerable<CancelledCompulsoryPolicyResponseDto>>> GetCancelledCompulsoryPoliciesAsync(CompulsoryCancelledPolicyRequestDto compulsoryCancelledPolicyRequest, CancellationToken cancellationToken);
            Task<ApiResponse<CancelledCompulsoryPolicyResponseDto>> GetCancelledCompulsoryPolicyByIDAsync(int id, CancellationToken cancellationToken);

            Task<string> GetUserPincodeByUserIdAsync(int userId, CancellationToken cancellationToken);
            Task<int> GetCancelledCompulsoryPoliciesCountAsync(CompulsoryCancelledPolicyCountRequestDto requestDto, CancellationToken cancellationToken);
            Task<ApiResponse> UpdateCancelledCompulsoryPolicyAsync(int id, CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto, CancellationToken cancellationToken);
       
    }
}
