using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Generators;

namespace PasswordGenerator.Tests;

public class PassphraseGeneratorTests
{
    private readonly PassphrasePasswordGenerator _generator;

    public PassphraseGeneratorTests()
    {
        var settings = Options.Create(new PasswordGeneratorSettings
        {
            Passphrase = new PassphraseSettings
            {
                WordListPath = GetWordListPath()
            }
        });
        _generator = new PassphrasePasswordGenerator(settings);
    }

    private static string GetWordListPath()
    {
        // Navigate up from test bin to find the wordlist
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "PasswordGenerator.sln")))
            dir = Directory.GetParent(dir)?.FullName;

        return Path.Combine(dir ?? ".",
            "src", "PasswordGenerator.Infrastructure", "WordList", "eff-large-wordlist.txt");
    }

    [Fact]
    public async Task Generate_ContainsSeparator()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions { WordCount = 4, Separator = "-" }
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Contains("-", results[0].Password);
    }

    [Fact]
    public async Task Generate_ReturnsRequestedCount()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Count = 5,
            Passphrase = new PassphraseOptions { WordCount = 4 }
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task Generate_EntropyIsPositive()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions { WordCount = 4 }
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.True(results[0].EntropyBits > 0);
    }

    [Fact]
    public async Task Generate_AppendNumberAndSymbol()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions
            {
                WordCount = 3,
                AppendNumber = true,
                AppendSymbol = true
            }
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        var password = results[0].Password;
        // Should end with a digit followed by a symbol
        Assert.True(char.IsDigit(password[^2]));
    }

    [Fact]
    public async Task Generate_CapitalizeFirst()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions
            {
                WordCount = 3,
                CapitalizeMode = "first",
                Separator = "-",
                AppendNumber = false,
                AppendSymbol = false
            }
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        var words = results[0].Password.Split('-');
        Assert.All(words, w => Assert.True(char.IsUpper(w[0])));
    }
}
