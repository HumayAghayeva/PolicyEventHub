using DataBridge.API.Abstraction;
using DataBridge.API.DTO;
using Microsoft.AspNetCore.Mvc;

namespace DataBridge.API.Controllers
{
    public class CancelledCompulsoryPoliciesController : BaseController<CancelledCompulsoryPoliciesController>
    {
        private readonly ICancelledCompulsaryPolicyService _compulsaryCancelledPolicyService;
        private readonly ILogger<CancelledCompulsoryPoliciesController> _logger;
        public CancelledCompulsoryPoliciesController(ICancelledCompulsaryPolicyService compulsaryCancelledPolicyService,
            ILogger<CancelledCompulsoryPoliciesController> logger)
        {
            _compulsaryCancelledPolicyService = compulsaryCancelledPolicyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ApiResponse<IEnumerable<CancelledCompulsoryPolicyResponseDto>>> GetCancelledCompulsoryPolicies([FromQuery] CompulsoryCancelledPolicyRequestDto compulsoryCancelledPolicyRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Method}: Starting. request: {@request}", nameof(GetCancelledCompulsoryPolicies), compulsoryCancelledPolicyRequest);

            var result = await _compulsaryCancelledPolicyService.GetCancelledCompulsoryPoliciesAsync(compulsoryCancelledPolicyRequest, cancellationToken);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<CancelledCompulsoryPolicyResponseDto>> GetCompulsoryCancelledPolicyById(int id, CancellationToken cancellationToken)
        {

            _logger.LogInformation("{Method}: Starting. request: {id}", nameof(GetCompulsoryCancelledPolicyById), id);

            var result = await _compulsaryCancelledPolicyService.GetCancelledCompulsoryPolicyByIDAsync(id, cancellationToken);

            return result;
        }
        [HttpGet("count")]
        public async Task<ApiResponse<int>> GetCancelledCompulsoryPoliciesCount([FromQuery] CompulsoryCancelledPolicyCountRequestDto compulsoryCancelledPolicyRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "{Method}: Starting. Request: {@Request}",
                nameof(GetCancelledCompulsoryPoliciesCount),
                compulsoryCancelledPolicyRequest);

            var count = await _compulsaryCancelledPolicyService
                .GetCancelledCompulsoryPoliciesCountAsync(compulsoryCancelledPolicyRequest, cancellationToken);

            return new ApiResponse<int>(count);
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateCancelledCompulsoryPolicyAsync([FromQuery] int id, CancelledCompulsoryPolicyUpdateDto compulsoryPolicyUpdateDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "{Method}: Starting. Request: {@Request}",
                nameof(UpdateCancelledCompulsoryPolicyAsync),
                compulsoryPolicyUpdateDto);

            return await _compulsaryCancelledPolicyService
                  .UpdateCancelledCompulsoryPolicyAsync(id, compulsoryPolicyUpdateDto, cancellationToken);
        }
    }

}
