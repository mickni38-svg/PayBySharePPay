using DataStorage.PayBySharePay.Entities;
using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IMerchantOrderFinalizationService
{
    Task ValidateAsync(
        Order order,
        IReadOnlyCollection<ParticipantPayment> payments,
        CancellationToken cancellationToken = default);

    Task<PayNSyncFinalGroupOrderDto> EnsureFinalizedAsync(
        Order order,
        IReadOnlyCollection<ParticipantPayment> payments,
        DateTime paidAtUtc,
        CancellationToken cancellationToken = default);
}
