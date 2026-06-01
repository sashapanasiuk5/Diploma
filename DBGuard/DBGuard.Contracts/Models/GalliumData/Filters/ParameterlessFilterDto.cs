using System.Text.Json.Serialization;
using DBGuard.Contracts.Models.GalliumData.Filters.Parameters;

namespace DBGuard.Contracts.Models.GalliumData.Filters;

public class ParameterlessFilterDto: BaseFilterDto
{
    [JsonPropertyName("parameters")]
    public Object? Parameters { get; set; } = null;
}