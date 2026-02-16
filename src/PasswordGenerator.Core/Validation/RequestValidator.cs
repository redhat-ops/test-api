using PasswordGenerator.Core.Models;

namespace PasswordGenerator.Core.Validation;

public static class RequestValidator
{
    public static (bool IsValid, string Code, string Message, List<string> Details) Validate(
        PasswordRequest request, PasswordGeneratorSettings settings)
    {
        var details = new List<string>();

        // Validate method
        var validMethods = new[] { "policy", "uniform", "passphrase" };
        if (!validMethods.Contains(request.Method, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "UNKNOWN_METHOD", "method must be policy|uniform|passphrase", details);
        }

        // Count enabled character sets
        int enabledSets = 0;
        if (request.IncludeLower) enabledSets++;
        if (request.IncludeUpper) enabledSets++;
        if (request.IncludeDigits) enabledSets++;
        if (request.IncludeSymbols) enabledSets++;

        // For passphrase, character set rules don't apply in the same way
        if (!string.Equals(request.Method, "passphrase", StringComparison.OrdinalIgnoreCase))
        {
            if (enabledSets == 0)
            {
                details.Add("At least one of includeLower, includeUpper, includeDigits, includeSymbols must be true");
                return (false, "VALIDATION_ERROR", "At least one character category must be enabled", details);
            }

            if (request.RequiredSets > enabledSets)
            {
                details.Add($"requiredSets={request.RequiredSets} but enabledSets={enabledSets}");
                return (false, "VALIDATION_ERROR", "requiredSets cannot exceed enabled character sets", details);
            }

            if (request.Length < settings.MinLength || request.Length > settings.MaxLength)
            {
                details.Add($"length={request.Length}, allowed range [{settings.MinLength}..{settings.MaxLength}]");
                return (false, "VALIDATION_ERROR",
                    $"length must be between {settings.MinLength} and {settings.MaxLength}", details);
            }

            if (request.RequiredSets > 1 && request.Length < request.RequiredSets)
            {
                details.Add($"length={request.Length} is less than requiredSets={request.RequiredSets}");
                return (false, "VALIDATION_ERROR",
                    "length must be >= requiredSets when requiredSets > 1", details);
            }
        }
        else
        {
            // Passphrase-specific validation
            var opts = request.Passphrase ?? new PassphraseOptions();
            if (opts.WordCount < settings.Passphrase.MinWordCount ||
                opts.WordCount > settings.Passphrase.MaxWordCount)
            {
                details.Add(
                    $"wordCount={opts.WordCount}, allowed range [{settings.Passphrase.MinWordCount}..{settings.Passphrase.MaxWordCount}]");
                return (false, "VALIDATION_ERROR",
                    $"wordCount must be between {settings.Passphrase.MinWordCount} and {settings.Passphrase.MaxWordCount}",
                    details);
            }
        }

        if (request.Count < 1 || request.Count > settings.MaxCount)
        {
            details.Add($"count={request.Count}, allowed range [1..{settings.MaxCount}]");
            return (false, "VALIDATION_ERROR",
                $"count must be between 1 and {settings.MaxCount}", details);
        }

        return (true, string.Empty, string.Empty, details);
    }
}
