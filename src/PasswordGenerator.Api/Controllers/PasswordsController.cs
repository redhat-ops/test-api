using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PasswordGenerator.Api.DTOs;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Core.Validation;

namespace PasswordGenerator.Api.Controllers;

[ApiController]
[Route("api/v1/passwords")]
[Produces("application/json")]
public class PasswordsController : ControllerBase
{
    private readonly IEnumerable<IPasswordGenerator> _generators;
    private readonly PasswordGeneratorSettings _settings;

    public PasswordsController(
        IEnumerable<IPasswordGenerator> generators,
        IOptions<PasswordGeneratorSettings> settings)
    {
        _generators = generators;
        _settings = settings.Value;
    }

    /// <summary>
    /// Generate passwords using the specified method (policy, uniform, or passphrase)
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateRequest request,
        CancellationToken ct)
    {
        var coreRequest = MapToCore(request);

        var (isValid, code, message, details) = RequestValidator.Validate(coreRequest, _settings);
        if (!isValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = code,
                    Message = message,
                    Details = details.Count > 0 ? details : null
                }
            });
        }

        var generator = _generators.FirstOrDefault(
            g => string.Equals(g.Method, request.Method, StringComparison.OrdinalIgnoreCase));

        if (generator is null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UNKNOWN_METHOD",
                    Message = "method must be policy|uniform|passphrase"
                }
            });
        }

        var results = await generator.GenerateAsync(coreRequest, ct);

        var response = new GenerateResponse
        {
            Method = request.Method.ToLowerInvariant(),
            GeneratedAtUtc = DateTime.UtcNow,
            Results = results.Select(r => new PasswordResultDto
            {
                Password = r.Password,
                EntropyBits = r.EntropyBits,
                PolicySatisfied = r.PolicySatisfied,
                Composition = new CompositionDto
                {
                    Lower = r.Composition.Lower,
                    Upper = r.Composition.Upper,
                    Digits = r.Composition.Digits,
                    Symbols = r.Composition.Symbols
                }
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Get service capabilities and configuration limits
    /// </summary>
    [HttpGet("capabilities")]
    [ProducesResponseType(typeof(CapabilitiesResponse), StatusCodes.Status200OK)]
    public IActionResult GetCapabilities()
    {
        var response = new CapabilitiesResponse
        {
            Methods = ["policy", "uniform", "passphrase"],
            Limits = new LimitsDto
            {
                MinLength = _settings.MinLength,
                MaxLength = _settings.MaxLength,
                MaxCount = _settings.MaxCount
            },
            SymbolSet = _settings.SymbolSet,
            ExcludedSimilarDefault = _settings.SimilarCharacters,
            Passphrase = new PassphraseCapabilitiesDto
            {
                MinWordCount = _settings.Passphrase.MinWordCount,
                MaxWordCount = _settings.Passphrase.MaxWordCount,
                DefaultSeparator = _settings.Passphrase.DefaultSeparator
            }
        };

        return Ok(response);
    }

    private static PasswordRequest MapToCore(GenerateRequest dto)
    {
        var request = new PasswordRequest
        {
            Method = dto.Method,
            Length = dto.Length,
            Count = dto.Count,
            IncludeLower = dto.IncludeLower,
            IncludeUpper = dto.IncludeUpper,
            IncludeDigits = dto.IncludeDigits,
            IncludeSymbols = dto.IncludeSymbols,
            ExcludeSimilar = dto.ExcludeSimilar,
            ExcludeAmbiguous = dto.ExcludeAmbiguous,
            RequiredSets = dto.RequiredSets
        };

        if (dto.Passphrase is not null)
        {
            request.Passphrase = new PassphraseOptions
            {
                WordCount = dto.Passphrase.WordCount,
                Separator = dto.Passphrase.Separator,
                CapitalizeMode = dto.Passphrase.CapitalizeMode,
                AppendNumber = dto.Passphrase.AppendNumber,
                AppendSymbol = dto.Passphrase.AppendSymbol
            };
        }

        return request;
    }
}
