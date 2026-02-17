namespace PolicyEventHub.Applications.DTOs
{
    public class OSAGORequestDto
    {
        public string SessionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? PIN { get; set; }
        public string? Plate { get; set; }
        public string? CertificationNumber { get; set; }
        public string? InsuredFullName { get; set; }
        public int UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
    }
}
