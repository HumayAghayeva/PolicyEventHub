namespace PolicyEventHub.Models.Api
{
    public class ErrorResponse
    {
        public string Code { get; set; } = null!;
        public string Message { get; set; } = null!;
        public List<ErrorDetail> Details { get; set; } = new();
    }
}
