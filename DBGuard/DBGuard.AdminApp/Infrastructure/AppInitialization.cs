using System.Text.Json;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.BLL.Services;
using DBGuard.Contracts.Models.RuleModels;
using DBGuard.Contracts.Models.RuleModels.RulesData;
using DBGuard.Contracts.Models.Settings;
using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.AdminApp.Infrastructure;

public class AppInitialization(IConfiguration configuration,
                                AppDbContext dbContext,
                                IGalliumRepositoryService repositoryService,
                                IHttpClientFactory httpClientFactory,
                                ILoginAnalyzerJobScheduler analyzerJobScheduler,
                                IEmailJobScheduler emailJobScheduler): IAppInitialization
{
    public async Task Initialize()
    {
        await InitializeDb();
        
        await InitializeGalliumProxy();

        var rules = await dbContext.Rules.ToListAsync();
        
        await repositoryService.InitProject(rules);

        await SetupJobs(rules);
    }
    
    private async Task InitializeDb()
    {
        await dbContext.Database.MigrateAsync();
        
        var rootExists = await dbContext.Users
            .AnyAsync(x => x.Role == UserRole.Root || x.Role == UserRole.RootFirstLogin);

        if (!rootExists)
        {
            var rootUser = new User
            {
                Username = "root",
                Email = "root@system.local",
                Password = BCrypt.Net.BCrypt.HashPassword("root"),
                Role = UserRole.RootFirstLogin,
                IsActive = false
            };

            dbContext.Users.Add(rootUser);
        }

        var sqlRuleExist = await dbContext.Rules.AnyAsync(x => x.Key == (byte)RuleType.SQLInjection);

        if (!sqlRuleExist)
        {
            var sqlInjectionRuleData = new SqlInjectionRuleData();

            dbContext.Rules.Add(new Rule()
            {
                Key = (byte)RuleType.SQLInjection,
                IsEnabled = false,
                Data = JsonSerializer.Serialize(sqlInjectionRuleData)
            });
        }
        
        var bruteForceRuleExist = await dbContext.Rules.AnyAsync(x => x.Key == (byte)RuleType.BruteForce);

        if (!bruteForceRuleExist)
        {
            var bruteForceRuleData = new BruteForceRuleData();

            dbContext.Rules.Add(new Rule()
            {
                Key = (byte)RuleType.BruteForce,
                IsEnabled = false,
                Data = JsonSerializer.Serialize(bruteForceRuleData)
            });
        }
        
        var bulkOperationsRuleExist = await dbContext.Rules.AnyAsync(x => x.Key == (byte)RuleType.BulkOperations);

        if (!bulkOperationsRuleExist)
        {
            var bulkOperationsRuleData = new List<BruteForceRuleData>();

            dbContext.Rules.Add(new Rule()
            {
                Key = (byte)RuleType.BulkOperations,
                IsEnabled = false,
                Data = JsonSerializer.Serialize(bulkOperationsRuleData)
            });
        }
        
        var emailSendingRuleExists = await dbContext.Rules.AnyAsync(x => x.Key == (byte)RuleType.MailSending);

        if (!emailSendingRuleExists)
        {
            var emailSendingRuleData = new EmailSendingRuleData();

            dbContext.Rules.Add(new Rule()
            {
                Key = (byte)RuleType.MailSending,
                IsEnabled = false,
                Data = JsonSerializer.Serialize(emailSendingRuleData)
            });
        }

        await dbContext.SaveChangesAsync();
    
    }

    private async Task InitializeGalliumProxy()
    {
        using var httpClient = httpClientFactory.CreateClient("GalliumData");
        bool isUp;
        try {
            var request = new HttpRequestMessage(HttpMethod.Head, httpClient.BaseAddress);
            var response = await httpClient.SendAsync(request);
            isUp = response.IsSuccessStatusCode;
        } catch (Exception ex) {
            Console.WriteLine("Could not connect to the Gallium Proxy");
            throw ex;
        }

        if (!isUp)
        {
            Console.WriteLine("Gallium Proxy is not available");
        }

    }

    private async Task SetupJobs(List<Rule> rules)
    {
        var bruteForceRuleModel = new BruteForceRuleModel(rules.Find(x => x.Key == (int)RuleType.BruteForce) ??
                                                          throw new InvalidOperationException());
        
        var emailSendingRuleModel = new EmailSendingRuleModel(rules.Find(x => x.Key == (int)RuleType.MailSending) ??
                                                          throw new InvalidOperationException());
        
        await analyzerJobScheduler.Sync(bruteForceRuleModel);
        await emailJobScheduler.Sync(emailSendingRuleModel);
    }
}