namespace PolicyEventHub.Models.Api
{
    public class ApiResponse<TData>
    {
        public TData? Data { get; init; }
        public Meta Meta { get; init; } = default!;
        public ErrorResponse? Error { get; init; }

        private ApiResponse(TData? data, Meta meta, ErrorResponse? error)
        {
            Data = data;
            Meta = meta;
            Error = error;
        }

        public static ApiResponse<TData> Success(
            TData data,
            string correlationId)
        {
            return new ApiResponse<TData>(
                data,
                new Meta
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId
                },
                error: null);
        }

        public static ApiResponse<EmptyData> SuccessWithoutData(string correlationId)
        {
            return new ApiResponse<EmptyData>(
                data: EmptyData.Instance,
                new Meta
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId
                },
                error: null);
        }

        public static ApiResponse<TData> Failure(
            string code,
            string message,
            IEnumerable<ErrorDetail> details,
            string correlationId)
        {
            return new ApiResponse<TData>(
                data: default,
                new Meta
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId
                },
                error: new ErrorResponse
                {
                    Code = code,
                    Message = message,
                    Details = details.ToList()
                });
        }
    }

}