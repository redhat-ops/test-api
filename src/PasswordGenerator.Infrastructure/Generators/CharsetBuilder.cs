using PasswordGenerator.Core.Models;

namespace PasswordGenerator.Infrastructure.Generators;

internal static class CharsetBuilder
{
    private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";

    public static (string pool, List<string> categories) Build(
        PasswordRequest request, PasswordGeneratorSettings settings)
    {
        var exclusions = new HashSet<char>();

        if (request.ExcludeSimilar)
            foreach (var c in settings.SimilarCharacters)
                exclusions.Add(c);

        if (request.ExcludeAmbiguous)
            foreach (var c in settings.AmbiguousCharacters)
                exclusions.Add(c);

        string Filter(string chars) =>
            new(chars.Where(c => !exclusions.Contains(c)).ToArray());

        var categories = new List<string>();
        var pool = string.Empty;

        if (request.IncludeLower)
        {
            var filtered = Filter(LowerChars);
            categories.Add(filtered);
            pool += filtered;
        }

        if (request.IncludeUpper)
        {
            var filtered = Filter(UpperChars);
            categories.Add(filtered);
            pool += filtered;
        }

        if (request.IncludeDigits)
        {
            var filtered = Filter(DigitChars);
            categories.Add(filtered);
            pool += filtered;
        }

        if (request.IncludeSymbols)
        {
            var filtered = Filter(settings.SymbolSet);
            categories.Add(filtered);
            pool += filtered;
        }

        return (pool, categories);
    }

    public static PasswordComposition Analyze(string password, PasswordGeneratorSettings settings)
    {
        var comp = new PasswordComposition();
        foreach (var c in password)
        {
            if (char.IsLower(c)) comp.Lower++;
            else if (char.IsUpper(c)) comp.Upper++;
            else if (char.IsDigit(c)) comp.Digits++;
            else if (settings.SymbolSet.Contains(c)) comp.Symbols++;
        }
        return comp;
    }

    public static double CalculateEntropy(int poolSize, int length)
    {
        if (poolSize <= 1) return 0;
        return length * Math.Log2(poolSize);
    }
}
