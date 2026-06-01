using System.Text.Json.Serialization;

namespace DBGuard.Contracts.Models.GalliumData.Filters.Parameters;

public class SqlInjectionParametersDto
{
    [JsonPropertyName("Action")]
    public int Action { get; set; }

    [JsonPropertyName("Threshold")]
    public string Threshold { get; set;  }
}