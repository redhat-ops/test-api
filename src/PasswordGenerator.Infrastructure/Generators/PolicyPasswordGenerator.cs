using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Helpers;

namespace PasswordGenerator.Infrastructure.Generators;

public class PolicyPasswordGenerator : IPasswordGenerator
{
    private readonly PasswordGeneratorSettings _settings;

    public PolicyPasswordGenerator(IOptions<PasswordGeneratorSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Method => "policy";

    public Task<IReadOnlyList<PasswordResult>> GenerateAsync(
        PasswordRequest request, CancellationToken ct)
    {
        var (pool, categories) = CharsetBuilder.Build(request, _settings);
        var results = new List<PasswordResult>(request.Count);

        for (int n = 0; n < request.Count; n++)
        {
            ct.ThrowIfCancellationRequested();

            var chars = new char[request.Length];
            int pos = 0;

            // Guarantee at least one character from each required category
            int setsToRequire = Math.Min(request.RequiredSets, categories.Count);
            for (int i = 0; i < setsToRequire; i++)
            {
                var cat = categories[i];
                chars[pos++] = cat[CryptoRng.Next(cat.Length)];
            }

            // Fill remaining positions from the full pool
            for (int i = pos; i < request.Length; i++)
            {
                chars[i] = pool[CryptoRng.Next(pool.Length)];
            }

            // Shuffle using crypto RNG
            CryptoRng.Shuffle(chars.AsSpan());

            var password = new string(chars);
            var composition = CharsetBuilder.Analyze(password, _settings);

            // Check policy satisfaction
            int satisfiedSets = 0;
            if (composition.Lower > 0 && request.IncludeLower) satisfiedSets++;
            if (composition.Upper > 0 && request.IncludeUpper) satisfiedSets++;
            if (composition.Digits > 0 && request.IncludeDigits) satisfiedSets++;
            if (composition.Symbols > 0 && request.IncludeSymbols) satisfiedSets++;

            results.Add(new PasswordResult
            {
                Password = password,
                EntropyBits = Math.Round(CharsetBuilder.CalculateEntropy(pool.Length, request.Length), 1),
                PolicySatisfied = satisfiedSets >= request.RequiredSets,
                Composition = composition
            });
        }

        return Task.FromResult<IReadOnlyList<PasswordResult>>(results);
    }
}
