using System.IO.Compression;
using System.Text.Json;
using DBGuard.BLL.Helpers;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.Common.Constants;
using DBGuard.Common.GalliumEntities;
using DBGuard.Common.Models;
using DBGuard.Contracts.Models.GalliumData;
using DBGuard.Contracts.Models.GalliumData.Filters;
using DBGuard.Contracts.Models.RuleModels;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DBGuard.BLL.Services;

public class GalliumRepositoryService(IHttpClientFactory factory, IProjectStructureFactory projectStructureFactory, IRuleRepository ruleRepository, IConfiguration configuration) : IGalliumRepositoryService
{
    private HttpClient GalliumDataClient => factory.CreateClient("GalliumData");
    
    private string RepositoryUri => GalliumDataClient.BaseAddress + $"zip/repository";
    public async Task InitProject(List<Rule> rules)
    {
        var response = await GalliumDataClient.GetAsync(RepositoryUri);
        var fileStream = await response.Content.ReadAsStreamAsync();
        
        string appPath = AppContext.BaseDirectory;
        string folderPath = Path.Combine(appPath, "repository");
        
        Directory.CreateDirectory(folderPath);

        using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(folderPath, overwriteFiles: true);

        var projectFolder = Path.Combine(folderPath, "repo","projects", GalliumConstants.ProjectName);
        string repoFilePath = Path.Combine(folderPath, "repo", "repository.json");

        if (!Directory.Exists(projectFolder))
        {
            //AddLibraries(repoFilePath);
            
            var project = new ProjectModel();
            var crypto = new CryptoModel();
            
            var builder = new SqlConnectionStringBuilder(configuration["DB_CONNECTION"]);
            
            var dataSource = builder.DataSource;

            string host;
            int port = 1433;

            if (dataSource.Contains(","))
            {
                var parts = dataSource.Split(',');
                host = parts[0];
                port = int.Parse((string)parts[1]);
            }
            else
            {
                host = dataSource;
            }
            
            var connection = new ConnectionModel()
            {
                Parameters = new ConnectionParametersModel()
                {
                    LocalPort = configuration.GetValue<int>(AppConfigKeys.GalliumDataLocalPort),
                    LocalAddress = null,
                    ServerHost = host,
                    ServerPort = port,
                    TrustServerCertificate = builder.TrustServerCertificate
                }
            };
            
            var filters = GetProjectFilters(rules);
            
            projectStructureFactory.CreateProjectStructure(Path.Combine(folderPath, "repo", "projects"), project, crypto, connection, filters);
            
            var stream = CreateZipStream(Path.Combine(folderPath, "repo"));

            var request = new HttpRequestMessage(HttpMethod.Post, RepositoryUri)
            {
                Content = new StreamContent(stream)
            };
            var responseMessage = await GalliumDataClient.SendAsync(request);
            
            Console.WriteLine((string?)responseMessage.StatusCode.ToString());
            Console.WriteLine((string?)responseMessage.Content.ReadAsStringAsync().Result);
        }
    }

    public async Task UpdateFilters(List<BaseFilterDto> filters)
    {
        string appPath = AppContext.BaseDirectory;
        string folderPath = Path.Combine(appPath, "repository");
        
        string projectPath = Path.Combine(folderPath, "repo","projects", GalliumConstants.ProjectName);

        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException(
                $"Project folder not found: {projectPath}");
        }

        foreach (BaseFilterDto filter in filters)
        {
            string jsonPath = FilterPathHelper.GetFilterJsonPath(projectPath, filter);

            string? directory = Path.GetDirectoryName(jsonPath);

            if (directory == null)
            {
                throw new InvalidOperationException(
                    $"Invalid path for filter: {filter.FilterType}");
            }

            Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(
                filter,
                filter.GetType());

            await File.WriteAllTextAsync(jsonPath, json);
        }
        
        var stream = CreateZipStream(Path.Combine(folderPath, "repo"));
        
        var request = new HttpRequestMessage(HttpMethod.Post, RepositoryUri)
        {
            Content = new StreamContent(stream)
        };
        var responseMessage = await GalliumDataClient.SendAsync(request);
    }

    public static MemoryStream CreateZipStream(string sourceFolder)
    {
        var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            string rootFolderName = Path.GetFileName(sourceFolder);

            // Explicit root entry
            archive.CreateEntry(rootFolderName + "/");

            // Explicit directory entries
            foreach (string directoryPath in Directory.GetDirectories(
                         sourceFolder,
                         "*",
                         SearchOption.AllDirectories))
            {
                string relativePath =
                    Path.GetRelativePath(sourceFolder, directoryPath)
                        .Replace("\\", "/");

                archive.CreateEntry(rootFolderName + "/" + relativePath + "/");
            }

            // File entries
            foreach (string filePath in Directory.GetFiles(
                         sourceFolder,
                         "*",
                         SearchOption.AllDirectories))
            {
                string relativePath =
                    Path.GetRelativePath(sourceFolder, filePath)
                        .Replace("\\", "/");

                archive.CreateEntryFromFile(
                    filePath,
                    rootFolderName + "/" + relativePath,
                    CompressionLevel.Optimal);
            }
        }
        
        string debugZipPath = Path.Combine(AppContext.BaseDirectory, "debug_generated.zip");

        using (var file = new FileStream(debugZipPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.Position = 0;
            memoryStream.CopyTo(file);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task AddLibraries(string repoFilePath)
    {
        string json = await File.ReadAllTextAsync(repoFilePath);
        
        Repository repo =
            JsonSerializer.Deserialize<Repository>(json)
            ?? new Repository();

        // Ensure Libraries collection exists
        repo.Libraries ??= new List<Library>();

        // Add new library
        repo.Libraries.Add(new Library
        {
            OrgId = "com.github",
            ArtifactId = "jsqlparser",
            Version = "4.6",
            Type = "java"
        });
        
        string updatedJson = JsonSerializer.Serialize(repo);
        
        await File.WriteAllTextAsync(repoFilePath, updatedJson);
    }

    private List<BaseFilterDto> GetProjectFilters(List<Rule> rules)
    {
        var sqlInjectionRule = new SqlInjectionRuleModel(rules.Find(x => x.Key == (int)AlertType.SQLInjection)!);
        
        var bulkOperationsRule = new BulkOperationsRuleModel(rules.Find(x => x.Key == (int)AlertType.BulkOperations)!);
        
        return
        [
            sqlInjectionRule.ToFilterDto(),
            bulkOperationsRule.ToFilterDto(),
            new ParameterlessFilterDto()
            {
                Active = true,
                FilterGroup = FilterGroup.Connection,
                FilterType = "ConnectionIPLogger"
            },
            new ParameterlessFilterDto()
            {
                Active = true,
                Priority = 99,
                FilterGroup = FilterGroup.Request,
                FilterType = "UsernameLogger"
            }
        ];
    }
}