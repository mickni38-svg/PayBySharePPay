using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IParticipantService
{
    Task<IEnumerable<ParticipantDto>> SearchParticipantsAsync(string query);
    Task<ParticipantDto> CreatePersonAsync(CreatePersonDto dto);
    Task<ParticipantDto> CreateMerchantAsync(CreateMerchantDto dto);
    Task AddFriendAsync(AddFriendDto dto);
}
