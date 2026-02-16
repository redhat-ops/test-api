namespace PasswordGenerator.Core.Models;

public class PasswordGeneratorSettings
{
    public string SymbolSet { get; set; } = "!@#$%^&*()-_=+[]{};:,.<>?";
    public string SimilarCharacters { get; set; } = "O0Il1|";
    public string AmbiguousCharacters { get; set; } = "{}[]()/'\"~,;:.<>";
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 256;
    public int MaxCount { get; set; } = 50;
    public PassphraseSettings Passphrase { get; set; } = new();
}

public class PassphraseSettings
{
    public string WordListPath { get; set; } = "WordList/eff-large-wordlist.txt";
    public int MinWordCount { get; set; } = 3;
    public int MaxWordCount { get; set; } = 8;
    public string DefaultSeparator { get; set; } = "-";
}
