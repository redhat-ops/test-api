using System.Text.Json.Serialization;

namespace PasswordGenerator.Api.DTOs;

public class CapabilitiesResponse
{
    [JsonPropertyName("methods")]
    public List<string> Methods { get; set; } = new();

    [JsonPropertyName("limits")]
    public LimitsDto Limits { get; set; } = new();

    [JsonPropertyName("symbolSet")]
    public string SymbolSet { get; set; } = string.Empty;

    [JsonPropertyName("excludedSimilarDefault")]
    public string ExcludedSimilarDefault { get; set; } = string.Empty;

    [JsonPropertyName("passphrase")]
    public PassphraseCapabilitiesDto Passphrase { get; set; } = new();
}

public class LimitsDto
{
    [JsonPropertyName("minLength")]
    public int MinLength { get; set; }

    [JsonPropertyName("maxLength")]
    public int MaxLength { get; set; }

    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }
}

public class PassphraseCapabilitiesDto
{
    [JsonPropertyName("minWordCount")]
    public int MinWordCount { get; set; }

    [JsonPropertyName("maxWordCount")]
    public int MaxWordCount { get; set; }

    [JsonPropertyName("defaultSeparator")]
    public string DefaultSeparator { get; set; } = string.Empty;
}
