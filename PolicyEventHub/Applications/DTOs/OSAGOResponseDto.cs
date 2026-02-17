namespace PolicyEventHub.Applications.DTOs
{
    public class OSAGOResponseDto
    {
        public int Id { get; set; }
        public string InsuredFullName { get; set; }
        public string PIN { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Plate { get; set; }
        public string DriverLicense { get; set; }
        public string CertificationNumber { get; set; }
        public string DocSeries { get; set; }
        public string DocNumber { get; set; }
        public string ContractNumber { get; set; }
        //public ContractStatus ContractStatus { get; set; } = ContractStatus.Active;
        //public string ContractStatusDesc => ContractStatus.ToString().ToLower();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
