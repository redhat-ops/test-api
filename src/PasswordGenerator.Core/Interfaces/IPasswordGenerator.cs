using PasswordGenerator.Core.Models;

namespace PasswordGenerator.Core.Interfaces;

public interface IPasswordGenerator
{
    string Method { get; }
    Task<IReadOnlyList<PasswordResult>> GenerateAsync(PasswordRequest request, CancellationToken ct);
}
