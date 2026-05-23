using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageService _messageService;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IParticipantRepository participantRepository,
        IOrderRepository orderRepository,
        IMessageService messageService)
    {
        _paymentRepository = paymentRepository;
        _participantRepository = participantRepository;
        _orderRepository = orderRepository;
        _messageService = messageService;
    }

    public async Task<PaymentDto> RegisterPaymentAsync(RegisterPaymentDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("En betaling skal have et gyldigt beløb større end 0.");

        var order = await _orderRepository.GetByIdWithDetailsAsync(dto.OrderId)
            ?? throw new KeyNotFoundException($"Ordre med id {dto.OrderId} findes ikke.");

        var participant = await _participantRepository.GetByIdAsync(dto.ParticipantId)
            ?? throw new KeyNotFoundException($"Deltager med id {dto.ParticipantId} findes ikke.");

        var payment = new Payment
        {
            OrderId = dto.OrderId,
            ParticipantId = dto.ParticipantId,
            Amount = dto.Amount,
            Status = "Completed"
        };

        await _paymentRepository.AddAsync(payment);
        await _paymentRepository.SaveChangesAsync();

        // Opdater deltagerens status til Paid
        var orderParticipant = order.OrderParticipants
            .FirstOrDefault(op => op.ParticipantId == dto.ParticipantId);
        if (orderParticipant != null)
        {
            orderParticipant.Status = "Paid";
            await _orderRepository.SaveChangesAsync();
        }

        // Send notifikation til host (besked i ordrechatten)
        var hostParticipantId = order.CreatedByParticipantId;
        if (hostParticipantId != dto.ParticipantId)
        {
            await _messageService.CreateMessageAsync(new CreateMessageDto
            {
                OrderId = dto.OrderId,
                ParticipantId = dto.ParticipantId,
                Content = $"✅ {participant.Name} har betalt {dto.Amount:N2} kr."
            });
        }

        return new PaymentDto
        {
            Id = payment.Id,
            ParticipantId = payment.ParticipantId,
            ParticipantName = participant.Name,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }
}
