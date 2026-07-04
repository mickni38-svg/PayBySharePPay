using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly IParticipantExternalLoginRepository _externalLoginRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IConfiguration _configuration;

    public ExternalAuthService(
        IParticipantExternalLoginRepository externalLoginRepository,
        IParticipantRepository participantRepository,
        IConfiguration configuration)
    {
        _externalLoginRepository = externalLoginRepository;
        _participantRepository = participantRepository;
        _configuration = configuration;
    }

    public async Task<ParticipantDto> GoogleLoginAsync(string idToken)
    {
        var googleClientId = _configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId er ikke konfigureret.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleClientId]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOperationException("Google ID-token er ugyldigt eller udløbet.", ex);
        }

        var providerUserId = payload.Subject;
        var email = payload.Email;
        var displayName = payload.Name ?? email ?? providerUserId;

        // Find eksisterende Google-tilknytning
        var existingLogin = await _externalLoginRepository.GetByProviderAsync("Google", providerUserId);
        if (existingLogin is not null)
        {
            var linked = await _participantRepository.GetByIdAsync(existingLogin.ParticipantId);
            return MapToDto(linked!);
        }

        // Tjek om e-mailen allerede er registreret med adgangskode (ingen auto-merge)
        if (!string.IsNullOrEmpty(email))
        {
            var existingByEmail = await _participantRepository.GetByEmailAsync(email);
            if (existingByEmail is not null && existingByEmail.PasswordHash is not null)
                throw new ExternalLoginEmailConflictException(email);
        }

        // Opret ny Participant
        var participant = new Participant
        {
            Type = ParticipantType.Person,
            Name = displayName,
            Email = email
        };
        await _participantRepository.AddAsync(participant);
        await _participantRepository.SaveChangesAsync();

        // Tilknyt Google-login
        var loginEntry = new ParticipantExternalLogin
        {
            ParticipantId = participant.Id,
            Provider = "Google",
            ProviderUserId = providerUserId,
            Email = email,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _externalLoginRepository.AddAsync(loginEntry);
        await _externalLoginRepository.SaveChangesAsync();

        return MapToDto(participant);
    }

    private static ParticipantDto MapToDto(Participant p) => new()
    {
        Id = p.Id,
        Type = p.Type.ToString(),
        Name = p.Name,
        Email = p.Email,
        Phone = p.Phone
    };
}
