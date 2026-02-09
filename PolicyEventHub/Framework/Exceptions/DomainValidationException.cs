using PolicyEventHub.Applications.Domain.Exceptions;

namespace PolicyEventHub.Framework.Exceptions
{
    public class DomainValidationException : DomainException
    {
        private const string DefaultErrorCode = "VALIDATION_ERROR";

        public DomainValidationException(string message)
            : base(message, DefaultErrorCode)
        {
        }

        public DomainValidationException(string message, string issue)
            : base(message, DefaultErrorCode, new[] { issue })
        {
        }

        public DomainValidationException(string message, IEnumerable<string>? issues)
            : base(message, DefaultErrorCode, issues)
        {
        }

        public DomainValidationException(string message, string errorCode, string issue)
            : base(message, errorCode, new[] { issue })
        {
        }

        public DomainValidationException(string message, string errorCode, IEnumerable<string> issues)
            : base(message, errorCode, issues)
        {
        }

        public DomainValidationException(string message, string errorCode, IEnumerable<string> issues, Exception innerException)
            : base(message, errorCode, issues, innerException)
        {
        }
    }
}
