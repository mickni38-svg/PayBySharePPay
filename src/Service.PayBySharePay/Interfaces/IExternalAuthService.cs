using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IExternalAuthService
{
    /// <summary>
    /// Validerer et Google ID-token og returnerer den tilknyttede Participant.
    /// Opretter automatisk en ny Participant hvis ingen eksisterer for denne udbyder.
    /// </summary>
    /// <exception cref="InvalidOperationException">Kastes hvis tokenet er ugyldigt.</exception>
    /// <exception cref="ExternalLoginEmailConflictException">Kastes hvis e-mailen allerede er i brug med en anden login-metode.</exception>
    Task<ParticipantDto> GoogleLoginAsync(string idToken);
}
