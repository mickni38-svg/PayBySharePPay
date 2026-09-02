using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Moq;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public class ParticipantServiceAccountTests
{
    [Fact]
    public async Task CreateMerchant_HashesPasswordAndStoresAccountEmail()
    {
        Participant? saved = null;
        var participantRepository = new Mock<IParticipantRepository>();
        participantRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Participant>()))
            .Callback<Participant>(participant =>
            {
                participant.Id = 42;
                saved = participant;
            })
            .ReturnsAsync((Participant participant) => participant);

        var service = new ParticipantService(
            participantRepository.Object,
            Mock.Of<IFriendRelationRepository>());

        var result = await service.CreateMerchantAsync(new CreateMerchantDto
        {
            Name = "Roma",
            CompanyName = "Roma ApS",
            Email = "merchant@paynsync.dk",
            Password = "secret42",
            VippsMerchantSerialNumber = "123456"
        });

        saved.Should().NotBeNull();
        saved!.Type.Should().Be(ParticipantType.Merchant);
        saved.Email.Should().Be("merchant@paynsync.dk");
        saved.PasswordHash.Should().NotBeNullOrWhiteSpace();
        saved.PasswordHash.Should().NotBe("secret42");
        service.VerifyPassword("secret42", saved.PasswordHash!).Should().BeTrue();
        result.PasswordHash.Should().Be(saved.PasswordHash);
    }

    [Theory]
    [InlineData("", "secret42", "123456")]
    [InlineData("merchant@paynsync.dk", "", "123456")]
    [InlineData("merchant@paynsync.dk", "secret42", "")]
    public async Task CreateMerchant_RejectsMissingAccountRequirements(
        string email,
        string password,
        string msn)
    {
        var service = new ParticipantService(
            Mock.Of<IParticipantRepository>(),
            Mock.Of<IFriendRelationRepository>());

        var act = () => service.CreateMerchantAsync(new CreateMerchantDto
        {
            Name = "Roma",
            CompanyName = "Roma ApS",
            Email = email,
            Password = password,
            VippsMerchantSerialNumber = msn
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
