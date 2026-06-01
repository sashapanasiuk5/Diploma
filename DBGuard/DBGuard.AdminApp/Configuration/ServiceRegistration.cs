using System.Text;
using DBGuard.AdminApp.AlertService;
using DBGuard.AdminApp.Infrastructure;
using DBGuard.AdminApp.Infrastructure.EmailHelpers;
using DBGuard.BLL.Helpers;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.BLL.Jobs;
using DBGuard.BLL.Services;
using DBGuard.Common.Constants;
using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Repositories;
using DBGuard.DataAccess.Repositories.Implementations;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace DBGuard.AdminApp.Configuration;

public static class ServiceRegistration
{
    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterInfrastructure(services, configuration);
        RegisterBLLServices(services);
        RegisterDataAccess(services, configuration);
        RegisterQuartz(services);
        RegisterAuthorization(services, configuration);
    }

    private static void RegisterBLLServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectStructureFactory, ProjectStructureFactory>();
        services.AddScoped<IGalliumRepositoryService, GalliumRepositoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAlertService, BLL.Services.AlertService>();
        services.AddScoped<IRuleService, RuleService>();

        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IJwtService, JwtService>();
    }

    private static void RegisterInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Account/Login");
        });
        services.AddScoped<IAppInitialization, AppInitialization>();

        services.AddScoped<IEmailRenderer, EmailRenderer>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddMemoryCache();
        
        services.AddHostedService<AlertHostedService>();
        
        services.AddHttpClient("GalliumData", client =>
        {
            var hostname = configuration[AppConfigKeys.GalliumDataHostName];
            client.BaseAddress = new Uri($"http://{hostname}");
        });

    }

    private static void RegisterDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(configuration[AppConfigKeys.DbConnectionString])
        );
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<ICheckpointRepository, CheckpointRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<ISqlAuditRepository>(x =>
            new SqlAuditRepository(configuration[AppConfigKeys.DbConnectionString]));
    }

    private static void RegisterAuthorization(IServiceCollection services, IConfiguration configuration)
    {
        
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"];
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        context.Response.Redirect("/Account/Login");

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();
    }

    private static void RegisterQuartz(IServiceCollection services)
    {
        services.AddScoped<ILoginAnalyzerJobScheduler, LoginAnalyzerJobScheduler>();
        services.AddScoped<IEmailJobScheduler, EmailJobScheduler>();
        services.AddQuartz();
        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });

    }
}