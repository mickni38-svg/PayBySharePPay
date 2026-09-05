using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public sealed class MerchantOrderFinalizationService(
    IMerchantOrderRepository merchantOrderRepository) : IMerchantOrderFinalizationService
{
    public Task ValidateAsync(
        Order order,
        IReadOnlyCollection<ParticipantPayment> payments,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = BuildFinalizationData(order, payments, requireCaptured: false);
        return Task.CompletedTask;
    }

    public async Task<PayNSyncFinalGroupOrderDto> EnsureFinalizedAsync(
        Order order,
        IReadOnlyCollection<ParticipantPayment> payments,
        DateTime paidAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await merchantOrderRepository.GetBySourceOrderIdAsync(order.Id, cancellationToken);
        if (existing is not null)
            return MapToDto(existing);

        if (order.Status != "Paid")
            throw new InvalidOperationException("Merchant-ordren kan kun oprettes for en betalt gruppeordre.");

        var merchantId = order.MerchantParticipantId
            ?? throw new InvalidOperationException("En betalt gruppeordre skal have en merchant.");

        var data = BuildFinalizationData(order, payments, requireCaptured: true);

        var merchantOrder = new MerchantOrder
        {
            SourceOrderId = order.Id,
            MerchantParticipantId = merchantId,
            PayNSyncOrderNumber = $"PNS-{order.Id:D8}",
            HostName = data.Host.Name,
            HostPhone = data.Host.Phone,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryPostalCode = order.DeliveryPostalCode,
            DeliveryCity = order.DeliveryCity,
            DeliveryCountry = order.DeliveryCountry,
            TotalAmount = data.TotalAmount,
            Currency = data.Currency,
            PaymentStatus = "Paid",
            PaidAtUtc = paidAtUtc,
            Items = data.SourceLines.Select(line => new MerchantOrderItem
            {
                Sku = line.LineId,
                Name = line.Name,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal
            }).ToList()
        };

        await merchantOrderRepository.AddAsync(merchantOrder, cancellationToken);
        await merchantOrderRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(merchantOrder);
    }

    private static FinalizationData BuildFinalizationData(
        Order order,
        IReadOnlyCollection<ParticipantPayment> payments,
        bool requireCaptured)
    {
        var merchantId = order.MerchantParticipantId
            ?? throw new InvalidOperationException("Gruppeordren skal have en merchant før merchant-ordren behandles.");

        var participantIds = order.OrderParticipants
            .Where(orderParticipant => orderParticipant.Participant.Type == ParticipantType.Person)
            .Select(orderParticipant => orderParticipant.ParticipantId)
            .Distinct()
            .ToList();

        var participantIdSet = participantIds.ToHashSet();

        var relevantPayments = payments
            .Where(payment => participantIdSet.Contains(payment.ParticipantId))
            .Where(payment => requireCaptured
                ? payment.Status == ParticipantPaymentStatus.Captured
                : payment.Status is ParticipantPaymentStatus.Reserved
                    or ParticipantPaymentStatus.CaptureFailed
                    or ParticipantPaymentStatus.Captured)
            .GroupBy(payment => payment.ParticipantId)
            .Select(group => group
                .OrderByDescending(payment => PaymentPriority(payment.Status))
                .ThenByDescending(payment => payment.CreatedAtUtc)
                .First())
            .ToList();

        var paidParticipantIds = relevantPayments.Select(payment => payment.ParticipantId).ToHashSet();
        if (participantIds.Count == 0 || participantIds.Any(id => !paidParticipantIds.Contains(id)))
        {
            var requiredStatus = requireCaptured ? "captured" : "reserveret eller captured";
            throw new InvalidOperationException($"Alle deltagere skal have en {requiredStatus} betaling før merchant-ordren behandles.");
        }

        var currencies = relevantPayments
            .Select(payment => payment.Currency.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (currencies.Count != 1)
            throw new InvalidOperationException("Alle captured betalinger skal have samme valuta.");

        var drafts = order.MerchantOrderDrafts
            .Where(draft => draft.MerchantParticipantId == merchantId
                            && draft.ParticipantId.HasValue
                            && paidParticipantIds.Contains(draft.ParticipantId.Value))
            .ToList();

        if (participantIds.Any(id => drafts.All(draft => draft.ParticipantId != id)))
            throw new InvalidOperationException("Alle captured deltagere skal have en merchant-draft.");

        if (drafts.Any(draft => !string.Equals(
                draft.Currency.Trim(),
                currencies[0],
                StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Merchant-drafts og captured betalinger skal have samme valuta.");

        var sourceLines = drafts.SelectMany(draft => draft.Lines).ToList();
        if (sourceLines.Count == 0)
            throw new InvalidOperationException("Merchant-ordren skal indeholde mindst én ordrelinje.");

        var paymentTotal = relevantPayments.Sum(payment => payment.AmountMinorUnits) / 100m;
        var lineTotal = sourceLines.Sum(line => line.LineTotal);
        if (lineTotal != paymentTotal)
            throw new InvalidOperationException(
                $"Summen af ordrelinjer ({lineTotal:0.00}) stemmer ikke med betalingsbeløbet ({paymentTotal:0.00}).");

        var host = order.OrderParticipants
            .FirstOrDefault(orderParticipant => orderParticipant.ParticipantId == order.CreatedByParticipantId)
            ?.Participant
            ?? order.CreatedBy
            ?? throw new InvalidOperationException("Ordreværten kunne ikke findes ved merchant-finalisering.");

        return new FinalizationData(host, sourceLines, paymentTotal, currencies[0]);
    }

    private static int PaymentPriority(ParticipantPaymentStatus status)
        => status switch
        {
            ParticipantPaymentStatus.Captured => 3,
            ParticipantPaymentStatus.CaptureFailed => 2,
            ParticipantPaymentStatus.Reserved => 1,
            _ => 0
        };

    private static PayNSyncFinalGroupOrderDto MapToDto(MerchantOrder order)
        => new()
        {
            PaynsyncOrderId = order.SourceOrderId,
            PaynsyncOrderNumber = order.PayNSyncOrderNumber,
            MerchantId = order.MerchantParticipantId,
            Status = order.PaymentStatus,
            Currency = order.Currency,
            TotalAmount = order.TotalAmount,
            PaidAtUtc = order.PaidAtUtc,
            Host = new PayNSyncHostDto
            {
                Name = order.HostName,
                Phone = order.HostPhone
            },
            DeliveryAddress = HasDeliveryAddress(order)
                ? new PayNSyncDeliveryAddressDto
                {
                    Address = order.DeliveryAddress,
                    PostalCode = order.DeliveryPostalCode,
                    City = order.DeliveryCity,
                    Country = order.DeliveryCountry
                }
                : null,
            Lines = order.Items.Select(item => new PayNSyncFinalOrderLineDto
            {
                Sku = item.Sku,
                Name = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToList()
        };

    private static bool HasDeliveryAddress(MerchantOrder order)
        => !string.IsNullOrWhiteSpace(order.DeliveryAddress)
           || !string.IsNullOrWhiteSpace(order.DeliveryPostalCode)
           || !string.IsNullOrWhiteSpace(order.DeliveryCity)
           || !string.IsNullOrWhiteSpace(order.DeliveryCountry);

    private sealed record FinalizationData(
        Participant Host,
        List<MerchantOrderLine> SourceLines,
        decimal TotalAmount,
        string Currency);
}
