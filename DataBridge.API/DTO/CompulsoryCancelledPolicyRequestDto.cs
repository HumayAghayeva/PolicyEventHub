namespace DataBridge.API.DTO
{
    public record CompulsoryCancelledPolicyRequestDto
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string Pin { get; init; }
        public string Plate { get; init; }
        public string CertificationNumber { get; init; }
        public int UserId { get; init; }
        public string InsuredFullName { get; init; }
        public int PageSize { get; init; }
        public int PageNumber { get; init; }
    }
}
