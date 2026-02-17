namespace PolicyEventHub.Applications.DTOs
{
    public sealed class AuthUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
