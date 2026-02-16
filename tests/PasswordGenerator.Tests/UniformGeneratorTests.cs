using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Generators;

namespace PasswordGenerator.Tests;

public class UniformGeneratorTests
{
    private readonly UniformPasswordGenerator _generator;

    public UniformGeneratorTests()
    {
        var settings = Options.Create(new PasswordGeneratorSettings());
        _generator = new UniformPasswordGenerator(settings);
    }

    [Fact]
    public async Task Generate_ReturnsCorrectLength()
    {
        var request = new PasswordRequest { Method = "uniform", Length = 32, Count = 1 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(32, results[0].Password.Length);
    }

    [Fact]
    public async Task Generate_ReturnsRequestedCount()
    {
        var request = new PasswordRequest { Method = "uniform", Length = 16, Count = 10 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task Generate_EntropyCalculated()
    {
        var request = new PasswordRequest { Method = "uniform", Length = 16 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.True(results[0].EntropyBits > 0);
    }

    [Fact]
    public async Task Generate_PasswordsAreUnique()
    {
        var request = new PasswordRequest { Method = "uniform", Length = 32, Count = 10 };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        var passwords = results.Select(r => r.Password).ToList();
        Assert.Equal(passwords.Count, passwords.Distinct().Count());
    }

    [Fact]
    public async Task Generate_OnlyLowerWhenConfigured()
    {
        var request = new PasswordRequest
        {
            Method = "uniform",
            Length = 50,
            Count = 1,
            IncludeLower = true,
            IncludeUpper = false,
            IncludeDigits = false,
            IncludeSymbols = false,
            RequiredSets = 1
        };
        var results = await _generator.GenerateAsync(request, CancellationToken.None);
        Assert.All(results[0].Password, c => Assert.True(char.IsLower(c)));
    }
}
