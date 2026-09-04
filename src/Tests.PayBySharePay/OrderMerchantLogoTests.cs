using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public class OrderMerchantLogoTests
{
    [Fact]
    public async Task GetOrdersByParticipant_IncludesMerchantLogoUrl()
    {
        var merchant = new Participant
        {
            Id = 42,
            Type = ParticipantType.Merchant,
            Name = "bella-napoli",
            CompanyName = "Bella Napoli",
            LogoImageData = [0x89, 0x50, 0x4E, 0x47]
        };
        var order = new Order
        {
            Id = 7,
            Title = "Fredag",
            MerchantParticipant = merchant,
            MerchantParticipantId = merchant.Id
        };
        var orders = new Mock<IOrderRepository>();
        orders.Setup(repository => repository.GetByParticipantIdAsync(5))
            .ReturnsAsync([order]);
        var service = new OrderService(
            orders.Object,
            Mock.Of<IParticipantRepository>(),
            Mock.Of<IParticipantPaymentRepository>(),
            new ConfigurationBuilder().Build());

        var result = (await service.GetOrdersByParticipantAsync(5)).Single();

        result.MerchantLogoUrl.Should().Be("/api/participants/42/logo");
    }
}
