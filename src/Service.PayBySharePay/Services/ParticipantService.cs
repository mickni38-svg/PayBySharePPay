using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class ParticipantService : IParticipantService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IFriendRelationRepository _friendRelationRepository;

    public ParticipantService(
        IParticipantRepository participantRepository,
        IFriendRelationRepository friendRelationRepository)
    {
        _participantRepository = participantRepository;
        _friendRelationRepository = friendRelationRepository;
    }

    public async Task<IEnumerable<ParticipantDto>> SearchParticipantsAsync(string query)
    {
        var participants = await _participantRepository.SearchAsync(query);
        return participants.Select(MapToDto);
    }

    public async Task<ParticipantDto> CreatePersonAsync(CreatePersonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("En person skal have et navn.");

        var participant = new Participant
        {
            Type = ParticipantType.Person,
            Name = dto.Name.Trim(),
            Email = dto.Email,
            Phone = dto.Phone
        };

        await _participantRepository.AddAsync(participant);
        await _participantRepository.SaveChangesAsync();

        return MapToDto(participant);
    }

    public async Task<ParticipantDto> CreateMerchantAsync(CreateMerchantDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            throw new ArgumentException("En merchant skal have et firmanavn.");

        var participant = new Participant
        {
            Type = ParticipantType.Merchant,
            Name = dto.Name.Trim(),
            CompanyName = dto.CompanyName.Trim(),
            CvrNumber = dto.CvrNumber,
            VatNumber = dto.VatNumber,
            ContactPerson = dto.ContactPerson,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            CompanyAddress = dto.CompanyAddress,
            PaymentReference = dto.PaymentReference,
            PayoutAccountInfo = dto.PayoutAccountInfo,
            PaymentProvider = dto.PaymentProvider
        };

        await _participantRepository.AddAsync(participant);
        await _participantRepository.SaveChangesAsync();

        return MapToDto(participant);
    }

    public async Task AddFriendAsync(AddFriendDto dto)
    {
        if (dto.InitiatorId == dto.ReceiverId)
            throw new InvalidOperationException("En bruger må ikke tilføje sig selv som ven.");

        var initiator = await _participantRepository.GetByIdAsync(dto.InitiatorId)
            ?? throw new KeyNotFoundException($"Deltager med id {dto.InitiatorId} findes ikke.");

        var receiver = await _participantRepository.GetByIdAsync(dto.ReceiverId)
            ?? throw new KeyNotFoundException($"Deltager med id {dto.ReceiverId} findes ikke.");

        var alreadyFriends = await _friendRelationRepository.RelationExistsAsync(dto.InitiatorId, dto.ReceiverId);
        if (alreadyFriends)
            throw new InvalidOperationException("Venrelationen eksisterer allerede.");

        var relation = new FriendRelation
        {
            InitiatorId = dto.InitiatorId,
            ReceiverId = dto.ReceiverId
        };

        await _friendRelationRepository.AddAsync(relation);
        await _friendRelationRepository.SaveChangesAsync();
    }

    private static ParticipantDto MapToDto(Participant p) => new()
    {
        Id = p.Id,
        Type = p.Type.ToString(),
        Name = p.Name,
        Email = p.Email,
        Phone = p.Phone,
        CompanyName = p.CompanyName
    };
}
