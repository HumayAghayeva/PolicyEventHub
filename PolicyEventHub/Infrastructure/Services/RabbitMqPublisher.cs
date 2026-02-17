using MassTransit;
using PolicyEventHub.Applications.Domain.Abstractions;
using PolicyEventHub.Applications.Observability;
using System.Diagnostics;

namespace PolicyEventHub.Infrastructure.Services
{
    public sealed class RabbitMqPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<RabbitMqPublisher> _logger;

        private readonly IAppMetrics _metrics;
        private readonly IAppTracer _tracer;

        public RabbitMqPublisher(
            IPublishEndpoint publishEndpoint,
            ILogger<RabbitMqPublisher> logger,
            IAppMetrics metrics,
            IAppTracer tracer)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;

            _metrics = metrics;
            _tracer = tracer;
        }

        public async Task PublishAsync<TMessage>(TMessage message,
         Action<MessagePublishOptions>? configure = null,
         CancellationToken ct = default)
         where TMessage : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var options = new MessagePublishOptions();
            configure?.Invoke(options);

            var messageType = typeof(TMessage).Name;
            var correlationId = options.CorrelationId ?? "<none>";
            var destination = options.Destination ?? "<none>";

            //using var span = _tracer.StartSpan(
            //    "RabbitMq.Publish",
            //    ActivityKind.Producer,
            //    new Dictionary<string, object?>
            //    {
            //        ["messaging.system"] = "rabbitmq",
            //        ["messaging.destination"] = destination,
            //        ["messaging.message_type"] = messageType,
            //        ["correlation.id"] = correlationId
            //    });

            using var timer = _metrics.MeasureUseCase("rabbitmq_publish");

            try
            {
                await _publishEndpoint.Publish(message, ctx =>
                {
                    if (!string.IsNullOrWhiteSpace(options.CorrelationId))
                    {
                        ctx.Headers.Set("X-Correlation-Id", options.CorrelationId);

                        if (Guid.TryParse(options.CorrelationId, out var guid))
                            ctx.CorrelationId = guid;
                    }

                    if (Activity.Current != null)
                    {
                        ctx.Headers.Set("traceparent", Activity.Current.Id);
                    }

                    foreach (var (key, value) in options.Headers)
                        ctx.Headers.Set(key, value);

                    if (!string.IsNullOrWhiteSpace(options.Destination))
                        ctx.SetRoutingKey(options.Destination);

                }, ct).ConfigureAwait(false);

                //_metrics.IncrementCounter(
                //    "rabbitmq_publish_total",
                //    new("status", "success"),
                //    new("message_type", messageType));

                _logger.LogInformation(
                    "Message published | Type={Type} | Destination={Destination} | CorrelationId={CorrelationId}",
                    messageType, destination, correlationId);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                //_metrics.IncrementCounter(
                //    "rabbitmq_publish_cancelled",
                //    new KeyValuePair<string, object?>("message_type", messageType));

                throw;
            }
            catch (Exception ex)
            {
                //_tracer.RecordException(span, ex);

                //_metrics.IncrementCounter(
                //    "rabbitmq_publish_total",
                //    new("status", "failed"),
                //    new("message_type", messageType));

                throw;
            }
        }
    }
}
