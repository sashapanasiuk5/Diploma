using System.Text.Json.Serialization;

namespace DBGuard.Contracts.Models.GalliumData.Filters;

public abstract class BaseFilterDto
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("filterType")]
    public string FilterType { get; set; } = String.Empty;

    [JsonPropertyName("phase")]
    public string Phase { get; set; } = "";

    [JsonPropertyName("priority")]
    public int Priority { get; set; }
    
    [JsonIgnore]
    public FilterGroup FilterGroup { get; set; }
}