namespace PasswordGenerator.Core.Models;

public class PasswordRequest
{
    public string Method { get; set; } = "policy";
    public int Length { get; set; } = 16;
    public int Count { get; set; } = 1;
    public bool IncludeLower { get; set; } = true;
    public bool IncludeUpper { get; set; } = true;
    public bool IncludeDigits { get; set; } = true;
    public bool IncludeSymbols { get; set; } = true;
    public bool ExcludeSimilar { get; set; } = true;
    public bool ExcludeAmbiguous { get; set; } = false;
    public int RequiredSets { get; set; } = 3;
    public PassphraseOptions? Passphrase { get; set; }
}
