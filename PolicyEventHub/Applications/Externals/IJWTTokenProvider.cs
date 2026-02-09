using PolicyEventHub.Applications.DTOs;

namespace PolicyEventHub.Applications.Externals
{
    public interface IJWTTokenProvider
    {
        Task<LoginResponse> RetrieveTokenAsync(string sessionId, CancellationToken ct);
    }
}
