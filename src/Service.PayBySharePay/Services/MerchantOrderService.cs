using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class MerchantOrderService : IMerchantOrderService
{
    private readonly IMerchantOrderDraftRepository _draftRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IParticipantRepository _participantRepository;

    public MerchantOrderService(
        IMerchantOrderDraftRepository draftRepository,
        IOrderRepository orderRepository,
        IParticipantRepository participantRepository)
    {
        _draftRepository = draftRepository;
        _orderRepository = orderRepository;
        _participantRepository = participantRepository;
    }

    public async Task<MerchantOrderDraftDto> InitOrderAsync(InitMerchantOrderDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(dto.OrderId)
            ?? throw new KeyNotFoundException($"Ordre med id {dto.OrderId} findes ikke.");

        var merchant = await _participantRepository.GetByIdAsync(dto.MerchantParticipantId)
            ?? throw new KeyNotFoundException($"Merchant med id {dto.MerchantParticipantId} findes ikke.");

        if (merchant.Type != ParticipantType.Merchant)
            throw new InvalidOperationException($"Deltager {dto.MerchantParticipantId} er ikke en merchant.");

        var draft = new MerchantOrderDraft
        {
            OrderId = dto.OrderId,
            MerchantParticipantId = dto.MerchantParticipantId,
            MerchantDraftReference = dto.MerchantDraftReference,
            SubtotalAmount = dto.SubtotalAmount,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency,
            PaymentMode = dto.PaymentMode,
            Status = "Collecting",
            ExpiresAtUtc = dto.ExpiresAtUtc,
            Lines = dto.Lines.Select(l => new MerchantOrderLine
            {
                LineId = l.LineId,
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        };

        await _draftRepository.AddAsync(draft);
        await _draftRepository.SaveChangesAsync();

        return MapToDto(draft);
    }

    public async Task<MerchantOrderDraftDto?> GetByOrderIdAsync(int orderId)
    {
        var draft = await _draftRepository.GetByOrderIdAsync(orderId);
        return draft is null ? null : MapToDto(draft);
    }

    private static MerchantOrderDraftDto MapToDto(MerchantOrderDraft d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        MerchantParticipantId = d.MerchantParticipantId,
        MerchantDraftReference = d.MerchantDraftReference,
        SubtotalAmount = d.SubtotalAmount,
        TotalAmount = d.TotalAmount,
        Currency = d.Currency,
        PaymentMode = d.PaymentMode,
        Status = d.Status,
        ExpiresAtUtc = d.ExpiresAtUtc,
        CreatedAtUtc = d.CreatedAtUtc,
        Lines = d.Lines.Select(l => new MerchantOrderLineDto
        {
            LineId = l.LineId,
            Name = l.Name,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal
        }).ToList()
    };
}
