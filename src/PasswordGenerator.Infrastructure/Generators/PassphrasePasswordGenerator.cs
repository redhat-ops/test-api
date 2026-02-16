using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Helpers;

namespace PasswordGenerator.Infrastructure.Generators;

public class PassphrasePasswordGenerator : IPasswordGenerator
{
    private readonly PasswordGeneratorSettings _settings;
    private readonly Lazy<string[]> _wordList;

    public PassphrasePasswordGenerator(IOptions<PasswordGeneratorSettings> settings)
    {
        _settings = settings.Value;
        _wordList = new Lazy<string[]>(() => LoadWordList());
    }

    public string Method => "passphrase";

    private string[] LoadWordList()
    {
        var path = _settings.Passphrase.WordListPath;
        if (!File.Exists(path))
            throw new FileNotFoundException($"Wordlist not found at: {path}");

        return File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'))
            .Select(line =>
            {
                // Support "12345\tword" (diceware) or plain word format
                var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : parts[0];
            })
            .ToArray();
    }

    public Task<IReadOnlyList<PasswordResult>> GenerateAsync(
        PasswordRequest request, CancellationToken ct)
    {
        var words = _wordList.Value;
        var opts = request.Passphrase ?? new PassphraseOptions();
        var results = new List<PasswordResult>(request.Count);

        var symbols = _settings.SymbolSet;

        for (int n = 0; n < request.Count; n++)
        {
            ct.ThrowIfCancellationRequested();

            var selectedWords = new string[opts.WordCount];
            for (int i = 0; i < opts.WordCount; i++)
            {
                var word = words[CryptoRng.Next(words.Length)];
                selectedWords[i] = opts.CapitalizeMode.ToLowerInvariant() switch
                {
                    "first" => char.ToUpper(word[0]) + word[1..],
                    "all" => word.ToUpper(),
                    _ => word
                };
            }

            var passphrase = string.Join(opts.Separator, selectedWords);

            if (opts.AppendNumber)
                passphrase += CryptoRng.Next(0, 10).ToString();

            if (opts.AppendSymbol)
                passphrase += symbols[CryptoRng.Next(symbols.Length)];

            var composition = CharsetBuilder.Analyze(passphrase, _settings);

            // Entropy: log2(wordListSize ^ wordCount), plus extras
            double entropy = opts.WordCount * Math.Log2(words.Length);
            if (opts.AppendNumber) entropy += Math.Log2(10);
            if (opts.AppendSymbol) entropy += Math.Log2(symbols.Length);

            results.Add(new PasswordResult
            {
                Password = passphrase,
                EntropyBits = Math.Round(entropy, 1),
                PolicySatisfied = true,
                Composition = composition
            });
        }

        return Task.FromResult<IReadOnlyList<PasswordResult>>(results);
    }
}
