using Api.PayBySharePay.Auth;
using Api.PayBySharePay.Controllers;
using Api.PayBySharePay.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Tests.PayBySharePay;

public class AuthControllerAccountTests
{
    [Theory]
    [InlineData("Simply")]
    [InlineData("Production")]
    public async Task Login_WithoutPassword_IsUnauthorizedOutsideDevelopment(string environmentName)
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("person@paynsync.dk"))
            .ReturnsAsync(Person(passwordHash: null));

        var controller = CreateController(participantService, environmentName);

        var result = await controller.Login(new LoginRequest
        {
            Email = "person@paynsync.dk"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
        participantService.Verify(
            service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_MissingPassword_CannotBypassExistingHashInDevelopment()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("person@paynsync.dk"))
            .ReturnsAsync(Person(passwordHash: "hash"));

        var controller = CreateController(participantService, "Development");

        var result = await controller.Login(new LoginRequest
        {
            Email = "person@paynsync.dk"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_PasswordlessSeedPerson_WorksOnlyInDevelopment()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("seed@paynsync.dk"))
            .ReturnsAsync(Person(passwordHash: null));

        var controller = CreateController(participantService, "Development");

        var result = await controller.Login(new LoginRequest
        {
            Email = "seed@paynsync.dk"
        });

        var response = result.Should().BeOfType<OkObjectResult>().Subject;
        var login = response.Value.Should().BeOfType<LoginResponse>().Subject;
        login.ParticipantType.Should().Be("Person");
        login.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_MerchantWithPassword_ReturnsMerchantSession()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("merchant@paynsync.dk"))
            .ReturnsAsync(Merchant(passwordHash: "hash"));
        participantService
            .Setup(service => service.VerifyPassword("secret42", "hash"))
            .Returns(true);

        var controller = CreateController(participantService, "Production");

        var result = await controller.Login(new LoginRequest
        {
            Email = "merchant@paynsync.dk",
            Password = "secret42"
        });

        var response = result.Should().BeOfType<OkObjectResult>().Subject;
        var login = response.Value.Should().BeOfType<LoginResponse>().Subject;
        login.ParticipantType.Should().Be("Merchant");
        login.ParticipantId.Should().Be(22);
    }

    [Fact]
    public async Task Login_PasswordlessMerchant_IsUnauthorizedInDevelopment()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("merchant@paynsync.dk"))
            .ReturnsAsync(Merchant(passwordHash: null));

        var controller = CreateController(participantService, "Development");

        var result = await controller.Login(new LoginRequest
        {
            Email = "merchant@paynsync.dk"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RegisterMerchant_PassesAccountCredentialsAndReturnsMerchantType()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("merchant@paynsync.dk"))
            .ReturnsAsync((ParticipantDto?)null);
        participantService
            .Setup(service => service.CreateMerchantAsync(It.IsAny<CreateMerchantDto>()))
            .ReturnsAsync(Merchant(passwordHash: null));

        var controller = CreateController(participantService, "Production");

        var result = await controller.RegisterMerchant(new RegisterMerchantRequest
        {
            Name = "Roma",
            CompanyName = "Roma ApS",
            Email = "merchant@paynsync.dk",
            Password = "secret42",
            VippsMerchantSerialNumber = "123456"
        });

        var response = result.Should().BeOfType<ObjectResult>().Subject;
        response.StatusCode.Should().Be(201);
        response.Value.Should().BeOfType<LoginResponse>()
            .Which.ParticipantType.Should().Be("Merchant");

        participantService.Verify(service => service.CreateMerchantAsync(
            It.Is<CreateMerchantDto>(dto =>
                dto.Email == "merchant@paynsync.dk" &&
                dto.Password == "secret42" &&
                dto.VippsMerchantSerialNumber == "123456")),
            Times.Once);
    }

    [Fact]
    public async Task RegisterMerchant_UsesConfiguredVippsCredentialsWhenEnabled()
    {
        var participantService = new Mock<IParticipantService>();
        participantService
            .Setup(service => service.GetByEmailAsync("merchant@paynsync.dk"))
            .ReturnsAsync((ParticipantDto?)null);
        participantService
            .Setup(service => service.CreateMerchantAsync(It.IsAny<CreateMerchantDto>()))
            .ReturnsAsync(Merchant(passwordHash: null));

        var controller = CreateController(
            participantService,
            "Simply",
            useDefaultVippsCredentials: true);

        var result = await controller.RegisterMerchant(new RegisterMerchantRequest
        {
            Name = "Roma",
            CompanyName = "Roma ApS",
            Email = "merchant@paynsync.dk",
            Password = "secret42"
        });

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);

        participantService.Verify(service => service.CreateMerchantAsync(
            It.Is<CreateMerchantDto>(dto =>
                dto.VippsMerchantSerialNumber == "TEST-MSN" &&
                dto.VippsClientId == "test-client-id" &&
                dto.VippsClientSecret == "test-client-secret" &&
                dto.VippsSubscriptionKey == "test-subscription-key")),
            Times.Once);
    }

    private static AuthController CreateController(
        Mock<IParticipantService> participantService,
        string environmentName,
        bool useDefaultVippsCredentials = false)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "uc-15-test-key-at-least-thirty-two-characters",
                ["Jwt:Issuer"] = "tests",
                ["Jwt:Audience"] = "tests",
                ["Jwt:ExpiresInMinutes"] = "60",
                ["Payments:VippsMobilePay:UseDefaultMerchantCredentialsOnRegistration"] = useDefaultVippsCredentials.ToString(),
                ["Payments:VippsMobilePay:MerchantSerialNumber"] = "TEST-MSN",
                ["Payments:VippsMobilePay:ClientId"] = "test-client-id",
                ["Payments:VippsMobilePay:ClientSecret"] = "test-client-secret",
                ["Payments:VippsMobilePay:SubscriptionKey"] = "test-subscription-key"
            })
            .Build();

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(environmentName);

        return new AuthController(
            participantService.Object,
            new JwtTokenService(configuration),
            Mock.Of<IExternalAuthService>(),
            environment.Object,
            configuration);
    }

    private static ParticipantDto Person(string? passwordHash) => new()
    {
        Id = 11,
        Type = "Person",
        Name = "Test Person",
        Email = "person@paynsync.dk",
        PasswordHash = passwordHash
    };

    private static ParticipantDto Merchant(string? passwordHash) => new()
    {
        Id = 22,
        Type = "Merchant",
        Name = "Roma",
        CompanyName = "Roma ApS",
        Email = "merchant@paynsync.dk",
        PasswordHash = passwordHash
    };
}
