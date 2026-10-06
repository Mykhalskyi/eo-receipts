namespace Receipts;

/// <summary>
/// A piece of a prompt. It prints itself into a media instead of exposing its content,
/// so every model can render parts in its own wire format without type checks.
/// </summary>
public interface IPart
{
    Task<IPartMedia<T>> Print<T>(IPartMedia<T> media, CancellationToken ct = default);
}
