namespace PolicyEventHub.Applications.DTOs
{
    public record CancelledCompulsoryPolicyUpdateDto
    {

        public string InsuredFullName { get; init; }
        public string PIN { get; init; }
        public string PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string Plate { get; init; }
        public string DriverLicense { get; init; }
        public string DocNumber { get; init; }
    }
}
