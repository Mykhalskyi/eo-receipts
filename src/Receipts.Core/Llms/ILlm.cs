namespace Receipts;

/// <summary>
/// A large language model able to look at images and answer with JSON.
/// </summary>
public interface ILlm
{
    Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default);

    /// <summary>
    /// Image types the model can read.
    /// </summary>
    IAcceptedTypes Accepts();
}
