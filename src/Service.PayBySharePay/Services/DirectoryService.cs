using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class DirectoryService : IDirectoryService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IFriendRelationRepository _friendRelationRepository;

    public DirectoryService(
        IParticipantRepository participantRepository,
        IFriendRelationRepository friendRelationRepository)
    {
        _participantRepository = participantRepository;
        _friendRelationRepository = friendRelationRepository;
    }

    public async Task<IEnumerable<DirectoryEntryDto>> SearchAsync(string query, int? excludeFriendsOf = null)
    {
        var participants = await _participantRepository.SearchAsync(query, excludeFriendsOf);
        return participants.Select(MapToEntry);
    }

    public async Task<IEnumerable<DirectoryEntryDto>> GetFriendsAsync(int participantId)
    {
        var friends = await _friendRelationRepository.GetFriendsOfAsync(participantId);
        return friends.Select(MapToEntry);
    }

    private static DirectoryEntryDto MapToEntry(Participant p)
    {
        if (p.Type == ParticipantType.Merchant)
        {
            return new DirectoryEntryDto
            {
                Id = p.Id,
                Type = "Merchant",
                DisplayName = p.CompanyName ?? p.Name,
                Handle = p.Name,
                Subtitle = p.CompanyAddress,
                LogoUrl = null,
                GroupOrderUrl = p.GroupOrderUrl
            };
        }

        return new DirectoryEntryDto
        {
            Id = p.Id,
            Type = "Person",
            DisplayName = p.Name,
            Handle = p.Email,
            Subtitle = null,
            LogoUrl = null
        };
    }
}
