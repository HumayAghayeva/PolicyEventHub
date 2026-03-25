namespace PolicyEventHub.Applications.DTOs
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessingStartedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessorId { get; set; }
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
        public DateTime? ProcessingDeadlineAt { get; set; }
    }
}
