namespace PolicyEventHub.Applications.Domain.Exceptions
{
    public class AppDomainException:DomainException
    {
        private const string DefaultErrorCode = "APP_ERROR";
        public AppDomainException(string message):base(message, DefaultErrorCode)
        {

        }
    }
}
