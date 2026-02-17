namespace PolicyEventHub.Applications.DTOs
{
    public record CompulsoryMotorSaleDto
    {
        public string Phone { get; init; }
        public string Email { get; init; }
        public string Plate { get; init; }
        public string Serial { get; init; }
        public string IdNumber { get; init; }
        public bool? IsWeb { get; init; }
        public bool LicenseId { get; init; } = false;
        public string LicenseNumber { get; init; }
        public string LicenseSerial { get; init; }
        public string Pin { get; init; }
    }
}
