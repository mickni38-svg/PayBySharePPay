using FluentAssertions;
using Infrastructure.Payments.PayBySharePay;
using Infrastructure.Payments.PayBySharePay.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Service.PayBySharePay.Interfaces;

namespace Tests.PayBySharePay;

public class FakePaymentProviderTests
{
    private static FakePaymentProvider CreateProvider()
        => new(NullLogger<FakePaymentProvider>.Instance);

    // ── DI ───────────────────────────────────────────────────────────────────

    [Fact]
    public void DI_CanResolve_IPaymentProvider_When_Provider_Is_Fake()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payments:Provider"] = "Fake"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentInfrastructure(config);

        var provider = services.BuildServiceProvider();
        var paymentProvider = provider.GetService<IPaymentProvider>();

        paymentProvider.Should().NotBeNull();
        paymentProvider.Should().BeOfType<FakePaymentProvider>();
    }

    [Fact]
    public void DI_CanResolve_IPaymentProvider_When_Provider_Is_Missing_Defaults_To_Fake()
    {
        var config = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentInfrastructure(config);

        var provider = services.BuildServiceProvider();
        var paymentProvider = provider.GetService<IPaymentProvider>();

        paymentProvider.Should().BeOfType<FakePaymentProvider>();
    }

    // ── Reserve ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reserve_Returns_Success_With_ProviderPaymentId()
    {
        var sut = CreateProvider();

        var result = await sut.ReserveAsync(new ReservePaymentRequest(
            GroupPaymentId: "grp-001",
            ParticipantPaymentId: "part-001",
            MerchantId: "merchant-001",
            AmountMinorUnits: 10000,
            Currency: "DKK",
            Description: "Test betaling",
            ReturnUrl: "https://app.local/return",
            CallbackUrl: "https://api.local/webhook",
            IdempotencyKey: Guid.NewGuid().ToString()));

        result.Success.Should().BeTrue();
        result.ProviderPaymentId.Should().NotBeNullOrEmpty();
        result.RedirectUrl.Should().NotBeNullOrEmpty();
        result.Status.Should().Be("Reserved");
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    // ── Capture ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Capture_Returns_Success_With_ProviderCaptureId()
    {
        var sut = CreateProvider();

        var result = await sut.CaptureAsync(new CapturePaymentRequest(
            ProviderPaymentId: "FAKE-abc123",
            AmountMinorUnits: 10000,
            Currency: "DKK",
            IdempotencyKey: Guid.NewGuid().ToString()));

        result.Success.Should().BeTrue();
        result.ProviderCaptureId.Should().NotBeNullOrEmpty();
        result.Status.Should().Be("Captured");
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_Returns_Success()
    {
        var sut = CreateProvider();

        var result = await sut.CancelAsync(new CancelPaymentRequest(
            ProviderPaymentId: "FAKE-abc123",
            Reason: "User cancelled",
            IdempotencyKey: Guid.NewGuid().ToString()));

        result.Success.Should().BeTrue();
        result.Status.Should().Be("Cancelled");
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    // ── GetStatus ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatus_Returns_Success_With_Status()
    {
        var sut = CreateProvider();

        var result = await sut.GetStatusAsync(new PaymentStatusRequest("FAKE-abc123"));

        result.Success.Should().BeTrue();
        result.Status.Should().NotBeNullOrEmpty();
        result.ReservedAmountMinorUnits.Should().BeGreaterThan(0);
    }
}
