using System.Text.Json.Serialization;
using DBGuard.Contracts.Models.GalliumData.Filters.Parameters;

namespace DBGuard.Contracts.Models.GalliumData.Filters;

public class SqlInjectionFilterDto: BaseFilterDto
{
    [JsonPropertyName("parameters")]
    public SqlInjectionParametersDto Parameters { get; set; } = default!;
}