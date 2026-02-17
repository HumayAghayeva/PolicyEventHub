namespace PolicyEventHub.Applications.Domain.Abstractions
{
    public sealed class MessagePublishOptions
    {
        /// <summary>
        /// Optional logical correlation id (HTTP correlation, job id, etc.).
        /// Will be forwarded as header and, if Guid, as MassTransit CorrelationId.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Logical destination / target system identifier.
        /// Infrastructure decides how to map this (routing key, topic, etc.).
        /// </summary>
        public string? Destination { get; set; }

        /// <summary>
        /// Custom headers to send with the message.
        /// </summary>
        public IDictionary<string, object> Headers { get; } =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
    public interface IEventPublisher
    {
        Task PublishAsync<TMessage>(
                TMessage message,
                Action<MessagePublishOptions>? configure = null,
                CancellationToken ct = default)
                where TMessage : class;
    }
}
