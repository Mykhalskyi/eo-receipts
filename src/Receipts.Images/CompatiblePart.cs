using System.Collections.Immutable;

namespace Receipts.Images;

/// <summary>
/// Part that prints its image through <see cref="CompatibleImage"/>.
/// </summary>
public sealed class CompatiblePart : IPart
{
    private readonly IPart origin;
    private readonly IAcceptedTypes types;

    public CompatiblePart(IPart origin, IAcceptedTypes types)
    {
        this.origin = origin;
        this.types = types;
    }

    public async Task<IPartMedia<T>> Print<T>(IPartMedia<T> media, CancellationToken ct = default)
    {
        var parts = await this.origin.Print(new PartsMedia(this.types), ct);
        var current = media;
        foreach (var part in parts.Value())
        {
            current = await part.Print(current, ct);
        }
        return current;
    }

    /// <summary>
    /// Re-creates printed parts, wrapping images into <see cref="CompatibleImage"/>.
    /// </summary>
    private sealed class PartsMedia : IPartMedia<IReadOnlyList<IPart>>
    {
        private readonly ImmutableList<IPart> parts;
        private readonly IAcceptedTypes types;

        public PartsMedia(IAcceptedTypes types) : this(ImmutableList<IPart>.Empty, types)
        {
        }

        private PartsMedia(ImmutableList<IPart> parts, IAcceptedTypes types)
        {
            this.parts = parts;
            this.types = types;
        }

        public Task<IPartMedia<IReadOnlyList<IPart>>> Text(string text, CancellationToken ct = default)
        {
            return Task.FromResult<IPartMedia<IReadOnlyList<IPart>>>(new PartsMedia(this.parts.Add(new TextPart(text)), this.types));
        }

        public Task<IPartMedia<IReadOnlyList<IPart>>> Image(IImage image, CancellationToken ct = default)
        {
            return Task.FromResult<IPartMedia<IReadOnlyList<IPart>>>(new PartsMedia(this.parts.Add(new ImagePart(new CompatibleImage(image, this.types))), this.types));
        }

        public IReadOnlyList<IPart> Value()
        {
            return this.parts;
        }
    }
}
