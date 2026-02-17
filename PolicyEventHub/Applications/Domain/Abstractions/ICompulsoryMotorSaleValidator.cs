using PolicyEventHub.Applications.DTOs;

namespace PolicyEventHub.Applications.Domain.Abstractions
{
    public interface ICompulsoryMotorSaleValidator
    {
        Task ValidateSendAsync(CompulsoryMotorSaleDto compulsoryMotorSaleDto ,CancellationToken ct);
    }
}
