namespace PasswordGenerator.Core.Models;

public class PassphraseOptions
{
    public int WordCount { get; set; } = 4;
    public string Separator { get; set; } = "-";
    public string CapitalizeMode { get; set; } = "first"; // none, first, all
    public bool AppendNumber { get; set; } = true;
    public bool AppendSymbol { get; set; } = true;
}
