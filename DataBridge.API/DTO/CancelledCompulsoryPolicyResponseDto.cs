namespace DataBridge.API.DTO
{
    public record CancelledCompulsoryPolicyResponseDto
    {
        public int Id { get; init; }
        public string InsuredFullName { get; init; }
        public string PIN { get; init; }
        public string PhoneNumber { get; init; }
        public string Email { get; init; }
        public string Plate { get; init; }
        public string DriverLicense { get; init; }
        public string CertificationNumber { get; init; }
        public string ContractNumber { get; init; }
        public string ContractStatus { get; init; }
        public string DocNumber { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
