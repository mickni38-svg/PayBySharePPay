namespace Service.PayBySharePay.DTOs;

/// <summary>
/// Kompakt oversigt over en ordre til brug i dashboardlisten.
/// Indeholder titel, kategori, status og en kort deltager-liste med betalingsstatus.
/// </summary>
public class OrderSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CreatedByParticipantId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? MerchantName { get; set; }

    /// <summary>Deltagere med navn og betalingsstatus</summary>
    public List<OrderParticipantDto> Participants { get; set; } = new();
}
