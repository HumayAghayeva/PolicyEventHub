namespace PolicyEventHub.Applications.DTOs
{
    public class LoginResponse
    {
        public int UserId {  get; set; }        
        public string FullName { get; set; }
        public string Token { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
