using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataStorage.PayBySharePay.Extensions;

public static class DataStorageServiceExtensions
{
    public static IServiceCollection AddDataStorage(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PayBySharePayDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<IFriendRelationRepository, FriendRelationRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}
