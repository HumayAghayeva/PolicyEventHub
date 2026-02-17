using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyEventHub.Applications.Domain.Abstractions;
using PolicyEventHub.Applications.DTOs;
using PolicyEventHub.Extensions;
using PolicyEventHub.Models.Api;

namespace PolicyEventHub.Controllers
{       /// <summary>
        /// This API provides comprehensive data retrieval from the CIBM database, supporting both full dataset queries and ID-specific lookups.
        /// </summary>
        /// <returns></returns>
        /// 
        [ApiController]
        [AllowAnonymous]
        [Route("api/v1/[controller]")]
        public class OSAGOController : ControllerBase
        {
            private readonly IOSAGOProxyService _oSAGOProxyService;
            public OSAGOController(IOSAGOProxyService oSAGOProxyService)
            {
                _oSAGOProxyService = oSAGOProxyService;
            }

            /// <summary>
            /// Retrieves a paginated list of cancelled compulsory insurance (OSAGO) policies 
            /// based on the provided filter criteria.
            /// </summary>
            /// <param name="oSAGORequestDto">
            /// Filtering and pagination parameters for the cancelled OSAGO policies, such as:
            /// <list type="bullet">
            /// <item>Policy number</item>
            /// <item>Insured person or company details</item>
            /// <item>Date range for cancellation</item>
            /// <item>Pagination parameters (page number, page size)</item>
            /// </list>
            /// </param>
            /// <param name="ct">Cancellation token to cancel the request.</param>
            /// <returns>
            /// Paginated list of cancelled OSAGO policies including:
            /// <list type="bullet">
            /// <item>Policy information (number, insured, insurer, dates)</item>
            /// <item>Cancellation details (reason, date)</item>
            /// <item>Metadata for tracking requests (correlation ID)</item>
            /// </list>
            /// </returns>
            /// <response code="200">Cancelled policies successfully retrieved.</response>
            /// <response code="400">Invalid request parameters.</response>
            [HttpGet]
            [ProducesResponseType(typeof(ApiResponse<PagedResultDto<OSAGOResponseDto>>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ApiResponse<PagedResultDto<OSAGOResponseDto>>> GetCancelledCompulsoryPoliciesAsync([FromQuery] OSAGORequestDto oSAGORequestDto,
             CancellationToken ct)
            {
                var result = await _oSAGOProxyService.GetFilteredCancelledCompulsoryPoliciesAsync(oSAGORequestDto, ct);

                return ApiResponse<PagedResultDto<OSAGOResponseDto>>.Success(result, this.ControllerContext.HttpContext.GetCorrelationId());
            }


            /// <summary>
            /// Retrieves details of a specific cancelled compulsory insurance (OSAGO) policy by its unique identifier.
            /// </summary>
            /// <param name="id">
            /// Unique identifier of the cancelled OSAGO policy to retrieve.
            /// </param>
            /// <param name="ct">Cancellation token to cancel the request.</param>
            /// <returns>
            /// Detailed information about the cancelled OSAGO policy, including:
            /// <list type="bullet">
            /// <item>Policy number and insured person or company</item>
            /// <item>Insurer information</item>
            /// <item>Cancellation details (reason, date)</item>
            /// <item>Policy metadata and timestamps</item>
            /// </list>
            /// </returns>
            /// <response code="200">Cancelled policy successfully retrieved.</response>
            /// <response code="404">Cancelled policy not found.</response>
            /// 
            [HttpGet("{id:int}")]
            [ProducesResponseType(typeof(ApiResponse<PagedResultDto<OSAGOResponseDto>>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ApiResponse<OSAGOResponseDto>> GetCancelledCompulsoryPolicyByIdAsync(int id, CancellationToken ct)
            {
                var result = await _oSAGOProxyService.GetCancelledCompulsoryPolicyByIdAsync(id, ct);

                return ApiResponse<OSAGOResponseDto>.Success(result, this.ControllerContext.HttpContext.GetCorrelationId());
            }

            /// <summary>
            /// Generates an iframe URL for accessing details of an OSAGO policy 
            /// for users who are not registered in the system.
            /// </summary>
            /// <param name="id">
            /// Unique identifier of the OSAGO policy.
            /// </param>
            /// <param name="ct">Cancellation token to cancel the request.</param>
            /// <returns>
            /// A URL string that can be used inside an iframe to display the OSAGO policy details for unregistered users.
            /// </returns>
            /// <response code="200">Iframe URL successfully generated.</response>
            /// <response code="400">Invalid request parameters.</response>
            [HttpPost]
            [ProducesResponseType(typeof(ApiResponse<PagedResultDto<OSAGOResponseDto>>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ApiResponse<string>> GetIframeUrlUnregisteredAsync(int id, CancellationToken ct)
            {
                var iframeUrl = await _oSAGOProxyService.GetIframeUrlUnregisteredAsync(id, ct);

                return ApiResponse<string>.Success(iframeUrl, this.ControllerContext.HttpContext.GetCorrelationId());
            }
            /// <summary>
            /// Updates the details of a cancelled compulsory insurance (OSAGO) policy.
            /// </summary>
            /// <param name="id">
            /// Unique identifier of the cancelled OSAGO policy to update.
            /// </param>
            /// <param name="cancelledCompulsoryPolicyUpdateDto">
            /// Object containing the fields to update for the cancelled policy.
            /// </param>
            /// <param name="ct">Cancellation token to cancel the request.</param>
            /// <returns>
            /// Returns an empty response indicating the update was successful, along with a correlation ID for tracking.
            /// </returns>
            /// <response code="204">Policy successfully updated. No content is returned.</response>
            /// <response code="400">Invalid request parameters or validation errors.</response>
            /// <response code="404">Cancelled policy not found.</response>
            [HttpPut]
            [ProducesResponseType(typeof(ApiResponse<EmptyData>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ApiResponse<EmptyData>> UpdateCancelledCompulsoryPolicyAsync([FromQuery] int id, CancelledCompulsoryPolicyUpdateDto cancelledCompulsoryPolicyUpdateDto, CancellationToken ct)
            {
                await _oSAGOProxyService.UpdateCancelledCompulsoryPolicyAsync(id, cancelledCompulsoryPolicyUpdateDto, ct);

                return ApiResponse<EmptyData>.SuccessWithoutData(this.ControllerContext.HttpContext.GetCorrelationId());
            }
        }
    }
   
