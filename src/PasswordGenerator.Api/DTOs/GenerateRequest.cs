using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PasswordGenerator.Api.DTOs;

public class GenerateRequest
{
    /// <summary>
    /// Generation method: policy, uniform, or passphrase
    /// </summary>
    [Required]
    [JsonPropertyName("method")]
    public string Method { get; set; } = "policy";

    /// <summary>
    /// Total password length (min 8, max 256). Ignored for passphrase method.
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; set; } = 16;

    /// <summary>
    /// Number of passwords to return (max 50)
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("includeLower")]
    public bool IncludeLower { get; set; } = true;

    [JsonPropertyName("includeUpper")]
    public bool IncludeUpper { get; set; } = true;

    [JsonPropertyName("includeDigits")]
    public bool IncludeDigits { get; set; } = true;

    [JsonPropertyName("includeSymbols")]
    public bool IncludeSymbols { get; set; } = true;

    /// <summary>
    /// Exclude visually similar characters (O, 0, I, l, 1, |)
    /// </summary>
    [JsonPropertyName("excludeSimilar")]
    public bool ExcludeSimilar { get; set; } = true;

    /// <summary>
    /// Exclude ambiguous characters ({ } [ ] ( ) / \ ' " ~ , ; : . &lt; &gt;)
    /// </summary>
    [JsonPropertyName("excludeAmbiguous")]
    public bool ExcludeAmbiguous { get; set; } = false;

    /// <summary>
    /// Minimum number of enabled categories that must appear at least once
    /// </summary>
    [JsonPropertyName("requiredSets")]
    public int RequiredSets { get; set; } = 3;

    /// <summary>
    /// Passphrase-specific options (only used when method = passphrase)
    /// </summary>
    [JsonPropertyName("passphrase")]
    public PassphraseRequest? Passphrase { get; set; }
}

public class PassphraseRequest
{
    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; } = 4;

    [JsonPropertyName("separator")]
    public string Separator { get; set; } = "-";

    /// <summary>
    /// Casing mode: none, first, all
    /// </summary>
    [JsonPropertyName("capitalizeMode")]
    public string CapitalizeMode { get; set; } = "first";

    [JsonPropertyName("appendNumber")]
    public bool AppendNumber { get; set; } = true;

    [JsonPropertyName("appendSymbol")]
    public bool AppendSymbol { get; set; } = true;
}
