using System.Text.Json.Serialization;
using DBGuard.Contracts.Models.GalliumData.Filters.Parameters;

namespace DBGuard.Contracts.Models.GalliumData.Filters;

public class BulkOperationsFilterDto: BaseFilterDto
{
    [JsonPropertyName("parameters")]
    public BulkOperationsParametersDto Parameters { get; set; }
}