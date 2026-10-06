namespace Receipts.OpenAi;

/// <summary>
/// URL of an API method relative to the API root, tolerant to a missing trailing slash.
/// </summary>
public sealed class ChatAddress
{
    private readonly Uri root;
    private readonly string path;

    public ChatAddress(Uri root, string path)
    {
        this.root = root;
        this.path = path;
    }

    public Uri Uri()
    {
        return new Uri($"{this.root.ToString().TrimEnd('/')}/{this.path.TrimStart('/')}");
    }
}
