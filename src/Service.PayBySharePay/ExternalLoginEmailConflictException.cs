namespace Service.PayBySharePay;

/// <summary>
/// Kastes når en ekstern login-e-mail allerede er registreret via en anden metode (f.eks. email/password),
/// og vi ikke ønsker at auto-merge konti.
/// </summary>
public class ExternalLoginEmailConflictException : Exception
{
    public ExternalLoginEmailConflictException(string email)
        : base($"E-mailen '{email}' er allerede tilknyttet en konto oprettet med adgangskode. Log ind med adgangskode for at tilknytte Google til din konto.")
    {
        Email = email;
    }

    public string Email { get; }
}
