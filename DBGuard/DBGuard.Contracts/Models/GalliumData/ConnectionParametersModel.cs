using System.Text.Json.Serialization;

namespace DBGuard.Common.Models;

public class ConnectionParametersModel
{
    [JsonPropertyName("Server host")]
    public string? ServerHost { get; set; }

    [JsonPropertyName("Local address")]
    public string? LocalAddress { get; set; }

    [JsonPropertyName("Server port")]
    public int ServerPort { get; set; }

    [JsonPropertyName("Local port")]
    public int LocalPort { get; set; }

    [JsonPropertyName("Trust server certificate")]
    public bool TrustServerCertificate { get; set; }

    [JsonPropertyName("Timeout to server")]
    public int? TimeoutToServer { get; set; } = null;

    [JsonPropertyName("Result set batch size (rows)")]
    public int? ResultSetBatchSizeRows { get; set; } = null;

    [JsonPropertyName("Result set batch size (bytes)")]
    public int? ResultSetBatchSizeBytes { get; set; } = null;
}