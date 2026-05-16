using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.EntityFrameworkCore;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class MerchantOrderService : IMerchantOrderService
{
    private readonly IMerchantOrderDraftRepository _draftRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IOrderService _orderService;
    private readonly PayBySharePayDbContext _db;

    public MerchantOrderService(
        IMerchantOrderDraftRepository draftRepository,
        IOrderRepository orderRepository,
        IParticipantRepository participantRepository,
        IOrderService orderService,
        PayBySharePayDbContext db)
    {
        _draftRepository = draftRepository;
        _orderRepository = orderRepository;
        _participantRepository = participantRepository;
        _orderService = orderService;
        _db = db;
    }

    public async Task<MerchantOrderDraftDto> InitOrderAsync(InitMerchantOrderDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(dto.OrderId)
            ?? throw new KeyNotFoundException($"Ordre med id {dto.OrderId} findes ikke.");

        var merchant = await _participantRepository.GetByIdAsync(dto.MerchantParticipantId)
            ?? throw new KeyNotFoundException($"Merchant med id {dto.MerchantParticipantId} findes ikke.");

        if (merchant.Type != ParticipantType.Merchant)
            throw new InvalidOperationException($"Deltager {dto.MerchantParticipantId} er ikke en merchant.");

        // Valider participantToken og find OrderParticipant
        var orderParticipant = await _db.OrderParticipants
            .Include(op => op.Participant)
            .FirstOrDefaultAsync(op => op.OrderId == dto.OrderId && op.ParticipantToken == dto.ParticipantToken);

        if (orderParticipant == null)
            throw new UnauthorizedAccessException("Ugyldigt participantToken for denne ordre.");

        if (orderParticipant.Participant.Type == ParticipantType.Merchant)
            throw new InvalidOperationException("En merchant kan ikke indsende en deltagerbestilling.");

        // Slet eventuel eksisterende draft for samme deltager (re-submit)
        var existing = await _db.MerchantOrderDrafts
            .Where(d => d.OrderId == dto.OrderId && d.ParticipantId == orderParticipant.ParticipantId)
            .FirstOrDefaultAsync();
        if (existing != null)
        {
            _db.MerchantOrderDrafts.Remove(existing);
            await _db.SaveChangesAsync();
        }

        var draft = new MerchantOrderDraft
        {
            OrderId = dto.OrderId,
            MerchantParticipantId = dto.MerchantParticipantId,
            ParticipantId = orderParticipant.ParticipantId,
            MerchantDraftReference = dto.MerchantDraftReference,
            SubtotalAmount = dto.SubtotalAmount,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency,
            PaymentMode = dto.PaymentMode,
            Status = "Submitted",
            ExpiresAtUtc = dto.ExpiresAtUtc,
            Lines = dto.Lines.Select(l => new MerchantOrderLine
            {
                LineId = l.LineId,
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal,
                ParticipantId = orderParticipant.ParticipantId
            }).ToList()
        };

        await _draftRepository.AddAsync(draft);
        await _draftRepository.SaveChangesAsync();

        // Opdater deltagerens status til OrderSubmitted
        orderParticipant.Status = "OrderSubmitted";
        await _db.SaveChangesAsync();

        // Tjek om alle deltagere har indsendt — sæt ReadyToPay hvis ja
        await _orderService.CheckAndSetReadyToPayAsync(dto.OrderId);

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
            ParticipantId = l.ParticipantId,
            LineId = l.LineId,
            Name = l.Name,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal
        }).ToList()
    };
}
