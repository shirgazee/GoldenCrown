using GoldenCrown.Database;
using GoldenCrown.Infrastructure.Clients.ExchangeClient;
using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using GoldenCrown.Infrastructure.Locking;
using GoldenCrown.Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace GoldenCrown.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        return services;
    }

    public static IServiceCollection AddDistributedCaching(this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConfig = configuration["Redis:Configuration"];
        var redis = ConnectionMultiplexer.Connect(redisConfig!);
        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddStackExchangeRedisCache(o =>
        {
            o.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(redis);
            o.InstanceName = configuration["Redis:InstanceName"];
        });
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        
        return services;
    }

    public static IServiceCollection AddExchangeClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ExchangeClient>()
            .AddPolicyHandler(GetRetryPolicy());
        services.Configure<ExchangeClientSettings>(configuration.GetSection("ExchangeClient"));
        services.AddScoped<IExchangeClient, DistributedCachedExchangeClient>(sp =>
            new DistributedCachedExchangeClient(
                sp.GetRequiredService<ExchangeClient>(),
                sp.GetRequiredService<IDistributedCache>(),
                sp.GetRequiredService<IDistributedLock>(),
                sp.GetRequiredService<ILogger<DistributedCachedExchangeClient>>()
            ));
        
        return services;
    }

    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IMessageProducer, RabbiMqMessageProducer>();

        return services;
    }
    
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode is System.Net.HttpStatusCode.UnprocessableEntity)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt))); // 2 4 8 16 32 64
    }
}