using System.Text.Json.Serialization;

namespace DBGuard.Contracts.Models.GalliumData;

public class Library
{
    [JsonPropertyName("orgId")]
    public string? OrgId { get; set; }

    [JsonPropertyName("artifactId")]
    public string? ArtifactId { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}