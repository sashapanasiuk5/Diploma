using System.Text.Json.Serialization;
using DBGuard.Contracts.Models.GalliumData;

namespace DBGuard.Common.GalliumEntities;

public class Repository
{
    public string? RepositoryVersion { get; set; }

    public Dictionary<string, object>? SystemSettings { get; set; }

    // "libraries" may be absent in JSON
    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; } = new();
}