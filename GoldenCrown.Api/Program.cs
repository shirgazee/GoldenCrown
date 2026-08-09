using FluentValidation;
using GoldenCrown.Api.BackgroundServices;
using GoldenCrown.Api.Dtos;
using GoldenCrown.Api.Dtos.User;
using GoldenCrown.Api.Middlewares;
using GoldenCrown.Application.Features.User.UserLogin;
using GoldenCrown.Application.Services.Currency;
using GoldenCrown.Database;
using GoldenCrown.Infrastructure.Clients.ExchangeClient;
using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using GoldenCrown.Infrastructure.Locking;
using GoldenCrown.Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace GoldenCrown.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(UserLoginCommandHandler).Assembly);
            });

            builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
            builder.Services.Configure<ExchangeClientSettings>(builder.Configuration.GetSection("ExchangeClient"));

            builder.Services.AddScoped<ICurrencyService, CurrencyService>();

            builder.Services.AddHttpClient<ExchangeClient>()
                .AddPolicyHandler(GetRetryPolicy());
            builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();
            builder.Services.AddScoped<IExchangeClient, DistributedCachedExchangeClient>(sp =>
                new DistributedCachedExchangeClient(
                    sp.GetRequiredService<ExchangeClient>(),
                    sp.GetRequiredService<IDistributedCache>(),
                    sp.GetRequiredService<IDistributedLock>(),
                    sp.GetRequiredService<ILogger<DistributedCachedExchangeClient>>()
                ));

            builder.Services.AddSingleton<IMessageProducer, RabbiMqMessageProducer>();

            builder.Services.AddValidatorsFromAssemblyContaining<LoginRequest>();
            builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
            
            var redisConfig = builder.Configuration["Redis:Configuration"];
            var redis = ConnectionMultiplexer.Connect(redisConfig!);
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
            builder.Services.AddStackExchangeRedisCache(o =>
            {
                o.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(redis);
                o.InstanceName = builder.Configuration["Redis:InstanceName"];
            });

            builder.Services.AddHostedService<SessionCleanupService>();
            builder.Services.AddHostedService<OutboxService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenApi at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("ApiKey",
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Please enter into field your api token",
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<AuthorizationMiddleware>();

            app.MapControllers();

            MigrateDatabase(app);

            app.Run();
        }

        private static void MigrateDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
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
}