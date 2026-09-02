using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.PayBySharePay.Controllers;
using Api.PayBySharePay.DTOs;
using Api.PayBySharePay.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Tests.PayBySharePay;

public class OrdersControllerAuthorizationTests
{
    private readonly Mock<IOrderService> _orderService = new();
    private readonly Mock<IExternalPaymentService> _externalPaymentService = new();
    private readonly Mock<IGroupPaymentOrchestrationService> _orchestration = new();

    [Fact]
    public async Task ApproveOrder_Uses_NameIdentifier_And_Ignores_Body_Id()
    {
        _orchestration
            .Setup(x => x.ApproveAndCaptureAllAsync(10, 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApproveAndCaptureResult { AllCaptured = true, OrderStatus = "Paid" });

        var sut = CreateController(new Claim(ClaimTypes.NameIdentifier, "42"));

        var result = await sut.ApproveOrder(10, new ApproveOrderRequest
        {
            RequestingParticipantId = 999
        });

        result.Should().BeOfType<OkObjectResult>();
        _orchestration.Verify(
            x => x.ApproveAndCaptureAllAsync(10, 42, It.IsAny<CancellationToken>()),
            Times.Once);
        _orchestration.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CancelOrder_Uses_Sub_Claim_As_Fallback()
    {
        _orchestration
            .Setup(x => x.CancelOrderAsync(10, 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CancelOrderResult { Success = true, OrderStatus = "Cancelled" });

        var sut = CreateController(new Claim(JwtRegisteredClaimNames.Sub, "42"));

        var result = await sut.CancelOrder(10, new CancelOrderRequest
        {
            RequestingParticipantId = 999
        });

        result.Should().BeOfType<OkObjectResult>();
        _orchestration.Verify(
            x => x.CancelOrderAsync(10, 42, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteOrder_Uses_Jwt_Id_And_Ignores_Body_Id()
    {
        _orderService
            .Setup(x => x.CompleteOrderAsync(10, 42))
            .ReturnsAsync(new OrderDto { Id = 10, CreatedByParticipantId = 42, Status = "Completed" });

        var sut = CreateController(new Claim(ClaimTypes.NameIdentifier, "42"));

        var result = await sut.CompleteOrder(10, new CompleteOrderRequest
        {
            RequestingParticipantId = 999
        });

        result.Should().BeOfType<OkObjectResult>();
        _orderService.Verify(x => x.CompleteOrderAsync(10, 42), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task ApproveOrder_Returns_401_Without_Valid_User_Claim(string? claimValue)
    {
        var sut = claimValue is null
            ? CreateController()
            : CreateController(new Claim(ClaimTypes.NameIdentifier, claimValue));

        var result = await sut.ApproveOrder(10, new ApproveOrderRequest
        {
            RequestingParticipantId = 42
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
        _orchestration.Verify(
            x => x.ApproveAndCaptureAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PayOrder_Rejects_NonHost_Before_External_Charge()
    {
        _orderService
            .Setup(x => x.GetOrderOverviewAsync(10))
            .ReturnsAsync(new OrderOverviewDto
            {
                OrderId = 10,
                CreatedByParticipantId = 1,
                Title = "Pizza",
                TotalAmount = 100
            });

        var sut = CreateController(new Claim(ClaimTypes.NameIdentifier, "2"));

        var act = () => sut.PayOrder(10, new PayOrderRequest
        {
            RequestingParticipantId = 1,
            Amount = 100,
            Currency = "DKK"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _externalPaymentService.Verify(
            x => x.ChargeAsync(It.IsAny<ExternalPaymentRequest>()),
            Times.Never);
        _orderService.Verify(
            x => x.CompleteOrderAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PayOrder_Uses_Jwt_Id_For_Host_And_Complete()
    {
        _orderService
            .Setup(x => x.GetOrderOverviewAsync(10))
            .ReturnsAsync(new OrderOverviewDto
            {
                OrderId = 10,
                CreatedByParticipantId = 42,
                Title = "Pizza",
                TotalAmount = 100
            });
        _externalPaymentService
            .Setup(x => x.ChargeAsync(It.IsAny<ExternalPaymentRequest>()))
            .ReturnsAsync(new ExternalPaymentResult(true, "PAY-1"));
        _orderService
            .Setup(x => x.CompleteOrderAsync(10, 42))
            .ReturnsAsync(new OrderDto { Id = 10, CreatedByParticipantId = 42, Status = "Completed" });

        var sut = CreateController(new Claim(ClaimTypes.NameIdentifier, "42"));

        var result = await sut.PayOrder(10, new PayOrderRequest
        {
            RequestingParticipantId = 999,
            Amount = 100,
            Currency = "DKK"
        });

        result.Should().BeOfType<OkObjectResult>();
        _externalPaymentService.Verify(
            x => x.ChargeAsync(It.IsAny<ExternalPaymentRequest>()),
            Times.Once);
        _orderService.Verify(x => x.CompleteOrderAsync(10, 42), Times.Once);
    }

    [Fact]
    public void OrdersController_Remains_Authorized()
    {
        typeof(OrdersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    private OrdersController CreateController(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var controller = new OrdersController(
            _orderService.Object,
            _externalPaymentService.Object,
            _orchestration.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };

        return controller;
    }
}

public class ExceptionHandlingMiddlewareAuthorizationTests
{
    [Fact]
    public async Task UnauthorizedAccessException_Returns_403_Without_Internal_Detail()
    {
        const string internalMessage = "Sensitive host authorization detail";
        RequestDelegate next = _ => throw new UnauthorizedAccessException(internalMessage);
        var middleware = new ExceptionHandlingMiddleware(
            next,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        body.Should().Contain("Du har ikke adgang til denne handling.");
        body.Should().NotContain(internalMessage);
        body.Should().NotContain(nameof(UnauthorizedAccessException));
    }
}
