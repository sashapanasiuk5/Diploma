using System.Text.Json;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.Common.Models;
using DBGuard.Contracts.Models.GalliumData;
using DBGuard.Contracts.Models.GalliumData.Filters;

namespace DBGuard.BLL.Helpers;

public class ProjectStructureFactory: IProjectStructureFactory
{
    private readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        WriteIndented = true 
    };

    public void CreateProjectStructure(string basePath, ProjectModel project, CryptoModel crypto, ConnectionModel connection, List<BaseFilterDto> filters)
    {
        string projectPath = Path.Combine(basePath, project.ProjectName);
        Directory.CreateDirectory(projectPath);
        
        File.WriteAllText(Path.Combine(projectPath, "project.json"), JsonSerializer.Serialize(project, _jsonOptions));
        File.WriteAllText(Path.Combine(projectPath, "comments.md"), project.ProjectDescription);
        
        string connectionsPath = Path.Combine(projectPath, "connections");
        Directory.CreateDirectory(connectionsPath);
        
        File.WriteAllText(Path.Combine(connectionsPath, $"{connection.ConnectionName}.json"), JsonSerializer.Serialize(connection, _jsonOptions));
        File.WriteAllText(Path.Combine(connectionsPath, $"{connection.ConnectionName}.md"), connection.Description);
        
        string cryptoPath = Path.Combine(projectPath, "crypto");
        Directory.CreateDirectory(cryptoPath);
        File.WriteAllText(Path.Combine(cryptoPath, "crypto.json"), JsonSerializer.Serialize(crypto, _jsonOptions));
        
        foreach (BaseFilterDto filter in filters)
        {
            var (groupFolderName, jsonFileName) = FilterPathHelper.GetPathParts(filter.FilterGroup);
            
            string groupPath = Path.Combine(projectPath, groupFolderName);

            Directory.CreateDirectory(groupPath);
            
            string filterFolderPath = Path.Combine(groupPath, filter.FilterType);

            Directory.CreateDirectory(filterFolderPath);
            
            string jsonFilePath = Path.Combine(filterFolderPath, jsonFileName);

            string json = JsonSerializer.Serialize(
                filter,
                filter.GetType(),
                _jsonOptions);

            File.WriteAllText(jsonFilePath, json);
        }
    }
}