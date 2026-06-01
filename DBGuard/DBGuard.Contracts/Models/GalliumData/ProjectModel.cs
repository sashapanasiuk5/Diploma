using System.Text.Json.Serialization;
using DBGuard.Common.Constants;

namespace DBGuard.Common.Models;

public class ProjectModel
{
    [JsonPropertyName("active")] public bool Active { get; set; } = true;
    
    [JsonIgnore]
    public string ProjectName { get; set; } = GalliumConstants.ProjectName;
    
    [JsonIgnore]
    public string ProjectDescription { get; set; } = "Project was created by DBGuard application";
}