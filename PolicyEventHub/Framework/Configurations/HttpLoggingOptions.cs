namespace PolicyEventHub.Framework.Configurations
{
    public sealed class HttpLoggingOptions
    {
        public List<string> SensitiveHeaders { get; set; } = new()
        {
            "Authorization", "Cookie", "Set-Cookie", "X-Api-Key", "X-Auth-Token"
        };

        public List<string> BodyRedactionKeys { get; set; } = new()
        {
            "password", "pwd", "secret", "token", "access_token", "refresh_token", "client_secret"
        };

        public long MaxBodySizeToLogBytes { get; set; } = 64 * 1024; // 64 KB
        public bool LogRequestBody { get; set; } = true;
        public bool LogResponseBody { get; set; } = true;

        /// <summary>
        /// Paths that should be skipped (request.Path.StartsWithSegments).
        /// Configurable via appsettings.
        /// </summary>
        public List<string> SkipPathsStartsWith { get; set; } = new()
        {
            "/health",
            "/metrics"
        };

        /// <summary>
        /// If true, gRPC requests (application/grpc) are skipped.
        /// </summary>
        public bool SkipGrpc { get; set; } = true;

        /// <summary>
        /// Content types considered text-like. Used to decide whether to log bodies.
        /// </summary>
        public List<string> TextLikeContentTypes { get; set; } = new()
        {
            "application/json",
            "text/plain",
            "text/html",
            "text/xml",
            "application/xml",
            "application/x-www-form-urlencoded",
            "application/problem+json"
        };

        /// <summary>
        /// Runtime predicate (not bound from config). Built from SkipPathsStartsWith & SkipGrpc.
        /// </summary>
        public Func<HttpContext, bool> ShouldSkip { get; set; } = _ => false;
    }
}
