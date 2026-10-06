namespace Receipts;

public sealed class Currency : ICurrency
{
    private readonly string code;

    public Currency(string code)
    {
        this.code = code;
    }

    public string Code()
    {
        return this.code;
    }

    public override string ToString()
    {
        return this.code;
    }
}
