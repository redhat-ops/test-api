namespace PasswordGenerator.Core.Models;

public class PasswordResult
{
    public string Password { get; set; } = string.Empty;
    public double EntropyBits { get; set; }
    public bool PolicySatisfied { get; set; }
    public PasswordComposition Composition { get; set; } = new();
}

public class PasswordComposition
{
    public int Lower { get; set; }
    public int Upper { get; set; }
    public int Digits { get; set; }
    public int Symbols { get; set; }
}
