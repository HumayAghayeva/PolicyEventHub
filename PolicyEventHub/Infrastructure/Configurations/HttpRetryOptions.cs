namespace PolicyEventHub.Infrastructure.Configurations
{
    public class HttpRetryOptions
    {
        public int RetryCount { get; set; } = 3;
        public int BaseDelaySeconds { get; set; } = 1; // exponential base delay }
    }
}