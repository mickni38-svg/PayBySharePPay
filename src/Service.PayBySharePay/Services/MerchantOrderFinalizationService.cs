using System.Text.Json;
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
            OrderHubStatus = "New",
            Note = order.Message,
            PaidAtUtc = paidAtUtc,
            UpdatedAtUtc = paidAtUtc,
            Items = data.SourceLines.Select(line => new MerchantOrderItem
            {
                Sku = line.Line.LineId,
                Name = line.Line.Name,
                Quantity = line.Line.Quantity,
                UnitPrice = line.Line.UnitPrice,
                LineTotal = line.Line.LineTotal,
                ModifiersJson = line.Modifiers.Count == 0 ? null : JsonSerializer.Serialize(line.Modifiers)
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

        var sourceLines = drafts.SelectMany(BuildSourceLineSnapshots).ToList();
        if (sourceLines.Count == 0)
            throw new InvalidOperationException("Merchant-ordren skal indeholde mindst én ordrelinje.");

        var paymentTotal = relevantPayments.Sum(payment => payment.AmountMinorUnits) / 100m;
        var lineTotal = sourceLines.Sum(line => line.Line.LineTotal);
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
                LineTotal = item.LineTotal,
                Modifiers = DeserializeModifiers(item.ModifiersJson)
            }).ToList(),
            ExternalOrderNumber = order.ExternalOrderNumber
        };

    public async Task RecordExternalDeliveryAsync(
        int sourceOrderId,
        MerchantOrderDeliveryResultDto result,
        CancellationToken cancellationToken = default)
    {
        var merchantOrder = await merchantOrderRepository.GetBySourceOrderIdAsync(sourceOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Merchant-ordre for PayNSync ordre {sourceOrderId} findes ikke.");

        // Idempotens: et eksternt ordrenummer må aldrig erstattes af et andet.
        if (!string.IsNullOrWhiteSpace(merchantOrder.ExternalOrderNumber))
            return;

        merchantOrder.ExternalResponseJson = result.ResponseBody;
        if (result.Success && !string.IsNullOrWhiteSpace(result.ExternalOrderNumber))
            merchantOrder.ExternalOrderNumber = result.ExternalOrderNumber;

        await merchantOrderRepository.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<SourceLineSnapshot> BuildSourceLineSnapshots(MerchantOrderDraft draft)
    {
        var rawItems = ParseRawItems(draft.RawMerchantPayloadJson);

        foreach (var line in draft.Lines)
        {
            var matchIndex = rawItems.FindIndex(item =>
                string.Equals(item.ProductId, line.LineId, StringComparison.Ordinal)
                && item.Quantity == line.Quantity
                && item.UnitPrice == line.UnitPrice
                && item.LineTotal == line.LineTotal);

            if (matchIndex < 0)
            {
                yield return new SourceLineSnapshot(line, []);
                continue;
            }

            var match = rawItems[matchIndex];
            rawItems.RemoveAt(matchIndex);
            yield return new SourceLineSnapshot(line, match.Modifiers);
        }
    }

    private static List<RawItemSnapshot> ParseRawItems(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return [];

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return [];

            return items.EnumerateArray().Select(item =>
            {
                var modifiers = item.TryGetProperty("modifiers", out var modifierArray)
                    && modifierArray.ValueKind == JsonValueKind.Array
                    ? modifierArray.EnumerateArray().Select(modifier => new PayNSyncFinalModifierDto
                    {
                        Id = modifier.TryGetProperty("id", out var id) ? id.GetString() : null,
                        Name = modifier.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                        Price = modifier.TryGetProperty("price", out var price) && price.TryGetDecimal(out var value) ? value : 0m
                    }).ToList()
                    : [];

                return new RawItemSnapshot(
                    item.TryGetProperty("productId", out var productId) ? productId.GetString() : null,
                    item.TryGetProperty("quantity", out var quantity) && quantity.TryGetInt32(out var q) ? q : 0,
                    item.TryGetProperty("unitPrice", out var unitPrice) && unitPrice.TryGetDecimal(out var up) ? up : 0m,
                    item.TryGetProperty("lineTotal", out var lineTotal) && lineTotal.TryGetDecimal(out var lt) ? lt : 0m,
                    modifiers);
            }).ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<PayNSyncFinalModifierDto> DeserializeModifiers(string? modifiersJson)
    {
        if (string.IsNullOrWhiteSpace(modifiersJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<PayNSyncFinalModifierDto>>(modifiersJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool HasDeliveryAddress(MerchantOrder order)
        => !string.IsNullOrWhiteSpace(order.DeliveryAddress)
           || !string.IsNullOrWhiteSpace(order.DeliveryPostalCode)
           || !string.IsNullOrWhiteSpace(order.DeliveryCity)
           || !string.IsNullOrWhiteSpace(order.DeliveryCountry);

    private sealed record FinalizationData(
        Participant Host,
        List<SourceLineSnapshot> SourceLines,
        decimal TotalAmount,
        string Currency);

    private sealed record SourceLineSnapshot(
        MerchantOrderLine Line,
        List<PayNSyncFinalModifierDto> Modifiers);

    private sealed record RawItemSnapshot(
        string? ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal,
        List<PayNSyncFinalModifierDto> Modifiers);
}
