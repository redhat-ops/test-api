using System.Text.Json.Serialization;

namespace PasswordGenerator.Api.DTOs;

public class GenerateResponse
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public List<PasswordResultDto> Results { get; set; } = new();

    [JsonPropertyName("generatedAtUtc")]
    public DateTime GeneratedAtUtc { get; set; }
}

public class PasswordResultDto
{
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("entropyBits")]
    public double EntropyBits { get; set; }

    [JsonPropertyName("policySatisfied")]
    public bool PolicySatisfied { get; set; }

    [JsonPropertyName("composition")]
    public CompositionDto Composition { get; set; } = new();
}

public class CompositionDto
{
    [JsonPropertyName("lower")]
    public int Lower { get; set; }

    [JsonPropertyName("upper")]
    public int Upper { get; set; }

    [JsonPropertyName("digits")]
    public int Digits { get; set; }

    [JsonPropertyName("symbols")]
    public int Symbols { get; set; }
}
