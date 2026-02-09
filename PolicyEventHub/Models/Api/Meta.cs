namespace PolicyEventHub.Models.Api
{
    public class Meta
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string CorrelationId { get; set; } = null!;
    }
}
