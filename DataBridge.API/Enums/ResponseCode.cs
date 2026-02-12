namespace DataBridge.API.Enums
{
    public enum ResponseCode
    {
        Success = 2000,
        DataNotFound = 2001,
        InternalServerError = 2002,
        ValidationError = 2003,
        AuthorizationError = 2004,
        AuthenticationError = 2005,
        ServiceProviderError = 2006
    }
}
