using PasswordGenerator.Core.Models;
using PasswordGenerator.Core.Validation;

namespace PasswordGenerator.Tests;

public class ValidationTests
{
    private readonly PasswordGeneratorSettings _settings = new();

    [Fact]
    public void Validate_ValidPolicyRequest_ReturnsValid()
    {
        var request = new PasswordRequest { Method = "policy", Length = 16, RequiredSets = 3 };
        var (isValid, _, _, _) = RequestValidator.Validate(request, _settings);
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_UnknownMethod_ReturnsError()
    {
        var request = new PasswordRequest { Method = "invalid" };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("UNKNOWN_METHOD", code);
    }

    [Fact]
    public void Validate_NoCharacterSetsEnabled_ReturnsError()
    {
        var request = new PasswordRequest
        {
            Method = "policy",
            IncludeLower = false,
            IncludeUpper = false,
            IncludeDigits = false,
            IncludeSymbols = false
        };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_RequiredSetsExceedsEnabled_ReturnsError()
    {
        var request = new PasswordRequest
        {
            Method = "policy",
            IncludeLower = true,
            IncludeUpper = true,
            IncludeDigits = false,
            IncludeSymbols = false,
            RequiredSets = 4
        };
        var (isValid, code, _, details) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
        Assert.Contains(details, d => d.Contains("requiredSets=4"));
    }

    [Fact]
    public void Validate_LengthTooShort_ReturnsError()
    {
        var request = new PasswordRequest { Method = "uniform", Length = 3 };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_LengthTooLong_ReturnsError()
    {
        var request = new PasswordRequest { Method = "policy", Length = 500 };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_CountExceedsMax_ReturnsError()
    {
        var request = new PasswordRequest { Method = "policy", Count = 100 };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_PassphraseWordCountTooLow_ReturnsError()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions { WordCount = 1 }
        };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_PassphraseWordCountTooHigh_ReturnsError()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions { WordCount = 20 }
        };
        var (isValid, code, _, _) = RequestValidator.Validate(request, _settings);
        Assert.False(isValid);
        Assert.Equal("VALIDATION_ERROR", code);
    }

    [Fact]
    public void Validate_ValidPassphraseRequest_ReturnsValid()
    {
        var request = new PasswordRequest
        {
            Method = "passphrase",
            Passphrase = new PassphraseOptions { WordCount = 5 }
        };
        var (isValid, _, _, _) = RequestValidator.Validate(request, _settings);
        Assert.True(isValid);
    }
}
