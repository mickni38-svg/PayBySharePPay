using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class AddFriendRequest
{
    [Required]
    public int InitiatorId { get; set; }
    [Required]
    public int ReceiverId { get; set; }
}
