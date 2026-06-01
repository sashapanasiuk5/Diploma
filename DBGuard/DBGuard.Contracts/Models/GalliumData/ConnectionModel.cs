using System.Text.Json.Serialization;

namespace DBGuard.Common.Models;

public class ConnectionModel
{
    [JsonPropertyName("active")] public bool Active { get; set; } = true;

    [JsonPropertyName("adapterType")]
    public string AdapterType { get; set; } = "MSSQL";

    [JsonPropertyName("parameters")]
    public ConnectionParametersModel Parameters { get; set; } = new();

    [JsonIgnore]
    public string Description { get; set; } = "MSSQL Server Connection";
    
    [JsonIgnore]
    public string ConnectionName { get; set; } = "MSSQLServerConnection";

}