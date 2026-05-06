using Microsoft.Extensions.DependencyInjection;
using Service.PayBySharePay.Interfaces;
using Service.PayBySharePay.Services;

namespace Service.PayBySharePay.Extensions;

public static class ServiceLayerExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMerchantOrderService, MerchantOrderService>();

        return services;
    }
}
