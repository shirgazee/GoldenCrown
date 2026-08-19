using FluentValidation;
using GoldenCrown.Api.BackgroundServices;
using GoldenCrown.Api.Dtos.User;
using GoldenCrown.Api.Middlewares;
using GoldenCrown.Application.Features.User.UserLogin;
using GoldenCrown.Application.Services.Currency;
using GoldenCrown.Database;
using GoldenCrown.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace GoldenCrown.Api;

/// <summary>
/// Startup class for configuring services and the application pipeline.
/// </summary>
public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;
    public IConfiguration Configuration { get; set; }

    /// <summary>
    /// Dependency injection configuration for services used in the application.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddDatabase(Configuration)
            .AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(UserLoginCommandHandler).Assembly); })
            .AddValidatorsFromAssemblyContaining<LoginRequest>()
            .AddAutoMapper(_ => { }, typeof(Program).Assembly)
            .AddExchangeClient(Configuration)
            .AddScoped<ICurrencyService, CurrencyService>()
            .AddRabbitMQ(Configuration)
            .AddDistributedCaching(Configuration)
            .AddHostedService<SessionCleanupService>()
            .AddHostedService<OutboxService>()
            .AddControllers();

        services.AddEndpointsApiExplorer()
            .AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("ApiKey",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter into field your api token",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
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
    }

    /// <summary>
    /// Middleware configuration for the application pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<AuthorizationMiddleware>();

        app.UseEndpoints(e => e.MapControllers());

        MigrateDatabase(app);
    }

    private static void MigrateDatabase(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}