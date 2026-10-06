namespace Receipts;

/// <summary>
/// Image types a model is able to read.
/// </summary>
public interface IAcceptedTypes
{
    bool Contains(IMediaType type);
}
