namespace Receipts;

/// <summary>
/// What we ask a model: ordered parts (text and images) and the shape of the expected JSON.
/// </summary>
public interface IPrompt
{
    IReadOnlyList<IPart> Parts();

    IJsonSchema Schema();
}
