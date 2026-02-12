namespace DataBridge.API.Models
{
    public abstract class BaseMessage
    {
        public string CorrelationId { get; set; }
        public string ApplicationName { get; set; }
    }
}
