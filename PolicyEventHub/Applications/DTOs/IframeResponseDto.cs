namespace PolicyEventHub.Applications.DTOs
{
    public record IframeResponseDto
    {
        public string TransactionNumber { get; init; }
        public string Token { get; init; }
        public string Url { get; init; }
    }
}
