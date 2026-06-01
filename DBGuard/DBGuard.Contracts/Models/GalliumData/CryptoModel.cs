using System.Text.Json.Serialization;

namespace DBGuard.Common.Models;

public class CryptoModel
{
    [JsonPropertyName("Key algorithm")] public string KeyAlgorithm { get; set; } = "RSA";
}