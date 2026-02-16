using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Generators;

namespace PasswordGenerator.Tests;

public class PolicyGeneratorTests
{
    private readonly PolicyPasswordGenerator _generator;

    public PolicyGeneratorTests()
    {
        var settings = Options.Create(new PasswordGeneratorSettings());
        _generator = new PolicyPasswordGenerator(settings);
    }

    [Fact]
    public async Task Generate_ReturnsRequestedCount()
    {
        var request = new PasswordRequest { Method = "policy", Length = 16, Count = 5 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task Generate_ReturnsCorrectLength()
    {
        var request = new PasswordRequest { Method = "policy", Length = 24, Count = 1 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(24, results[0].Password.Length);
    }

    [Fact]
    public async Task Generate_AllRequiredSetsPresent()
    {
        var request = new PasswordRequest
        {
            Method = "policy",
            Length = 20,
            Count = 1,
            IncludeLower = true,
            IncludeUpper = true,
            IncludeDigits = true,
            IncludeSymbols = true,
            RequiredSets = 4
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        var comp = results[0].Composition;
        Assert.True(comp.Lower > 0);
        Assert.True(comp.Upper > 0);
        Assert.True(comp.Digits > 0);
        Assert.True(comp.Symbols > 0);
    }

    [Fact]
    public async Task Generate_PolicySatisfied_WhenRequiredSetsMet()
    {
        var request = new PasswordRequest
        {
            Method = "policy",
            Length = 20,
            Count = 1,
            RequiredSets = 3
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.True(results[0].PolicySatisfied);
    }

    [Fact]
    public async Task Generate_EntropyIsPositive()
    {
        var request = new PasswordRequest { Method = "policy", Length = 16 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.True(results[0].EntropyBits > 0);
    }

    [Fact]
    public async Task Generate_ExcludesSimilarCharacters()
    {
        var request = new PasswordRequest
        {
            Method = "policy",
            Length = 100,
            Count = 1,
            ExcludeSimilar = true
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        var similar = "O0Il1|";
        Assert.DoesNotContain(results[0].Password, c => similar.Contains(c));
    }
}
