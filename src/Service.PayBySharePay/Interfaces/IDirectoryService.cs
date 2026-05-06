using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IDirectoryService
{
    Task<IEnumerable<DirectoryEntryDto>> SearchAsync(string query, int? excludeFriendsOf = null);
}
