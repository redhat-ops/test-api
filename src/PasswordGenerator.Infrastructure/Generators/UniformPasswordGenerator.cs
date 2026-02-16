using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Helpers;

namespace PasswordGenerator.Infrastructure.Generators;

public class UniformPasswordGenerator : IPasswordGenerator
{
    private readonly PasswordGeneratorSettings _settings;

    public UniformPasswordGenerator(IOptions<PasswordGeneratorSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Method => "uniform";

    public Task<IReadOnlyList<PasswordResult>> GenerateAsync(
        PasswordRequest request, CancellationToken ct)
    {
        var (pool, _) = CharsetBuilder.Build(request, _settings);
        var results = new List<PasswordResult>(request.Count);

        for (int n = 0; n < request.Count; n++)
        {
            ct.ThrowIfCancellationRequested();

            var chars = new char[request.Length];
            for (int i = 0; i < request.Length; i++)
            {
                chars[i] = pool[CryptoRng.Next(pool.Length)];
            }

            var password = new string(chars);
            var composition = CharsetBuilder.Analyze(password, _settings);

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
