using System.Security.Cryptography;

namespace PasswordGenerator.Infrastructure.Helpers;

public static class CryptoRng
{
    public static int Next(int maxExclusive)
    {
        return RandomNumberGenerator.GetInt32(maxExclusive);
    }

    public static int Next(int minInclusive, int maxExclusive)
    {
        return RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);
    }

    public static void Shuffle<T>(Span<T> span)
    {
        for (int i = span.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (span[i], span[j]) = (span[j], span[i]);
        }
    }
}
