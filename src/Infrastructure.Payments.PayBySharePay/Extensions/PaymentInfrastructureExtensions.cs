using Infrastructure.Payments.PayBySharePay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.PayBySharePay.Interfaces;

namespace Infrastructure.Payments.PayBySharePay.Extensions;

public static class PaymentInfrastructureExtensions
{
    /// <summary>
    /// Registrerer <see cref="IPaymentProvider"/> baseret på konfigurationen <c>Payments:Provider</c>.
    /// Understøttede værdier: <c>Fake</c> (standard).
    /// Fremtidigt: <c>MobilePay</c>, <c>Vipps</c>.
    /// </summary>
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Payments:Provider"] ?? "Fake";

        switch (provider)
        {
            case "Fake":
            default:
                services.AddScoped<IPaymentProvider, FakePaymentProvider>();
                break;
        }

        return services;
    }
}
