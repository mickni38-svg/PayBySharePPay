using Infrastructure.Payments.PayBySharePay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;

namespace Infrastructure.Payments.PayBySharePay.Extensions;

public static class PaymentInfrastructureExtensions
{
    /// <summary>
    /// Registrerer <see cref="IPaymentProvider"/> baseret på konfigurationen <c>Payments:Provider</c>.
    /// Understøttede værdier: <c>Fake</c> (standard), <c>MobilePay</c>.
    /// </summary>
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Payments:Provider"] ?? "Fake";

        switch (provider)
        {
            case "MobilePay":
                var vippsOptions = configuration
                    .GetSection(VippsMobilePayOptions.SectionName)
                    .Get<VippsMobilePayOptions>() ?? new VippsMobilePayOptions();
                services.AddSingleton(vippsOptions);

                services.AddHttpClient<VippsMobilePayTokenService>();
                services.AddSingleton<VippsMobilePayTokenService>(sp =>
                    new VippsMobilePayTokenService(
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(VippsMobilePayTokenService)),
                        sp.GetRequiredService<VippsMobilePayOptions>(),
                        sp.GetRequiredService<ILogger<VippsMobilePayTokenService>>()));

                services.AddHttpClient<MobilePaySandboxPaymentProvider>();
                services.AddScoped<IPaymentProvider>(sp =>
                    new MobilePaySandboxPaymentProvider(
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(MobilePaySandboxPaymentProvider)),
                        sp.GetRequiredService<VippsMobilePayOptions>(),
                        sp.GetRequiredService<VippsMobilePayTokenService>(),
                        sp.GetRequiredService<ILogger<MobilePaySandboxPaymentProvider>>()));
                break;

            case "Fake":
            default:
                var section = configuration.GetSection(FakePaymentProviderOptions.SectionName);
                var fakeOptions = new FakePaymentProviderOptions
                {
                    SimulateReservationFailed  = bool.TryParse(section["SimulateReservationFailed"],  out var a) && a,
                    SimulateReservationExpired = bool.TryParse(section["SimulateReservationExpired"], out var b) && b,
                    SimulateCaptureFailed      = bool.TryParse(section["SimulateCaptureFailed"],      out var c) && c,
                    SimulateCancelFailed       = bool.TryParse(section["SimulateCancelFailed"],       out var d) && d,
                    SimulateReserveException   = bool.TryParse(section["SimulateReserveException"],   out var e) && e,
                    SimulateCaptureException   = bool.TryParse(section["SimulateCaptureException"],   out var f) && f,
                };
                services.AddSingleton(fakeOptions);
                services.AddScoped<IPaymentProvider, FakePaymentProvider>(sp =>
                    new FakePaymentProvider(
                        sp.GetRequiredService<ILogger<FakePaymentProvider>>(),
                        sp.GetRequiredService<FakePaymentProviderOptions>()));
                break;
        }

        return services;
    }
}
