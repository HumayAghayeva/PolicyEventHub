namespace PolicyEventHub.Applications.Domain.Exceptions
{
    public class DomainException:Exception
    {
        public string ErrorCode { get; }
        public IReadOnlyCollection<string> Issues { get; }

        protected DomainException(string message, string errorCode,
            IEnumerable<string>? issue = null,
            Exception? innerException = null): base(message, innerException)
        {
            ErrorCode = errorCode;
            Issues =issue?.ToArray() ?? Array.Empty<string>();    
        }
    }
}
