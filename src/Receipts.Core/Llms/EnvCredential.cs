namespace Receipts;

/// <summary>
/// Credential read from an environment variable when it is needed.
/// </summary>
public sealed class EnvCredential : ICredential
{
    private readonly string variable;

    public EnvCredential(string variable)
    {
        this.variable = variable;
    }

    public Task<string> Value(CancellationToken ct = default)
    {
        var value = Environment.GetEnvironmentVariable(this.variable);
        if (string.IsNullOrEmpty(value))
        {
            throw new LlmException("credential", 401, $"Environment variable {this.variable} is not set");
        }
        return Task.FromResult(value);
    }
}
