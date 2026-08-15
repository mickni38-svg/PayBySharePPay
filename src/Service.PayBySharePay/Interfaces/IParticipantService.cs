using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IParticipantService
{
    Task<IEnumerable<ParticipantDto>> SearchParticipantsAsync(string query, int? excludeFriendsOf = null);
    Task<IEnumerable<ParticipantDto>> GetFriendsAsync(int participantId);
    Task<ParticipantDto> CreatePersonAsync(CreatePersonDto dto);
    Task<ParticipantDto> CreateMerchantAsync(CreateMerchantDto dto);
    Task AddFriendAsync(AddFriendDto dto);
    Task<ParticipantDto?> GetByIdAsync(int id);
    Task<ParticipantDto> UpdateProfileAsync(UpdateProfileDto dto);
    Task<ParticipantDto?> GetByEmailAsync(string email);
    bool VerifyPassword(string password, string passwordHash);
    Task<IEnumerable<VippsTestPersonDto>> GetVippsTestPersonsAsync();
    Task SetVippsTestUserAsync(int participantId, int? vippsTestUserId);
    Task<MerchantLogoDto?> GetMerchantLogoAsync(int merchantId);
    Task UpdateMerchantLogoAsync(int merchantId, UpdateMerchantLogoDto dto);
}
