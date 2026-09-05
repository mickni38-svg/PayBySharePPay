using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public sealed class OrderHubService(
    IMerchantOrderRepository merchantOrderRepository,
    IParticipantRepository participantRepository) : IOrderHubService
{
    private static readonly IReadOnlyDictionary<string, string> NextStatus =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["New"] = "Accepted",
            ["Accepted"] = "Preparing",
            ["Preparing"] = "Ready",
            ["Ready"] = "Completed"
        };

    public async Task<OrderHubSettingsDto> GetSettingsAsync(int participantId, CancellationToken cancellationToken = default)
    {
        var merchant = await RequireMerchantAsync(participantId);
        return new OrderHubSettingsDto { Enabled = merchant.OrderHubEnabled };
    }

    public async Task<OrderHubSettingsDto> SetEnabledAsync(int participantId, bool enabled, CancellationToken cancellationToken = default)
    {
        var merchant = await RequireMerchantAsync(participantId);
        merchant.OrderHubEnabled = enabled;
        await participantRepository.UpdateAsync(merchant);
        return new OrderHubSettingsDto { Enabled = merchant.OrderHubEnabled };
    }

    public async Task<IReadOnlyList<OrderHubOrderDto>> GetActiveOrdersAsync(int participantId, CancellationToken cancellationToken = default)
    {
        var merchant = await RequireEnabledMerchantAsync(participantId);
        var orders = await merchantOrderRepository.GetByMerchantAsync(merchant.Id, completed: false, cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<OrderHubOrderDto>> GetHistoryAsync(int participantId, CancellationToken cancellationToken = default)
    {
        var merchant = await RequireEnabledMerchantAsync(participantId);
        var orders = await merchantOrderRepository.GetByMerchantAsync(merchant.Id, completed: true, cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderHubOrderDto> UpdateStatusAsync(
        int participantId,
        int merchantOrderId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var merchant = await RequireEnabledMerchantAsync(participantId);
        var order = await merchantOrderRepository.GetByIdAsync(merchantOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Merchant-ordre {merchantOrderId} findes ikke.");

        if (order.MerchantParticipantId != merchant.Id)
            throw new UnauthorizedAccessException("Merchant-ordren tilhører en anden merchant.");

        if (!NextStatus.TryGetValue(order.OrderHubStatus, out var expected)
            || !string.Equals(expected, newStatus?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Ugyldig Order Hub-statusovergang fra '{order.OrderHubStatus}' til '{newStatus}'.");

        order.OrderHubStatus = expected;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await merchantOrderRepository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    private async Task<Participant> RequireMerchantAsync(int participantId)
    {
        var participant = await participantRepository.GetByIdAsync(participantId)
            ?? throw new KeyNotFoundException($"Participant {participantId} findes ikke.");

        if (participant.Type != ParticipantType.Merchant)
            throw new UnauthorizedAccessException("Order Hub er kun tilgængelig for merchantkonti.");

        return participant;
    }

    private async Task<Participant> RequireEnabledMerchantAsync(int participantId)
    {
        var merchant = await RequireMerchantAsync(participantId);
        if (!merchant.OrderHubEnabled)
            throw new UnauthorizedAccessException("Order Hub-adgang er ikke aktiveret for merchantkontoen.");
        return merchant;
    }

    private static OrderHubOrderDto Map(MerchantOrder order)
        => new()
        {
            Id = order.Id,
            SourceOrderId = order.SourceOrderId,
            PayNSyncOrderNumber = order.PayNSyncOrderNumber,
            Status = order.OrderHubStatus,
            PaymentStatus = order.PaymentStatus,
            Currency = order.Currency,
            TotalAmount = order.TotalAmount,
            PaidAtUtc = order.PaidAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            HostName = order.HostName,
            HostPhone = order.HostPhone,
            Note = order.Note,
            DeliveryAddress = HasAddress(order)
                ? new OrderHubDeliveryAddressDto
                {
                    Address = order.DeliveryAddress,
                    PostalCode = order.DeliveryPostalCode,
                    City = order.DeliveryCity,
                    Country = order.DeliveryCountry
                }
                : null,
            Items = order.Items.Select(item => new OrderHubOrderItemDto
            {
                Sku = item.Sku,
                Name = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                ModifiersJson = item.ModifiersJson
            }).ToList()
        };

    private static bool HasAddress(MerchantOrder order)
        => !string.IsNullOrWhiteSpace(order.DeliveryAddress)
           || !string.IsNullOrWhiteSpace(order.DeliveryPostalCode)
           || !string.IsNullOrWhiteSpace(order.DeliveryCity)
           || !string.IsNullOrWhiteSpace(order.DeliveryCountry);
}
