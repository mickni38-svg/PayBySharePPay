namespace Service.PayBySharePay.DTOs;

public class DirectoryEntryDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Handle { get; set; }
    public string? Subtitle { get; set; }
    public string? LogoUrl { get; set; }
}
