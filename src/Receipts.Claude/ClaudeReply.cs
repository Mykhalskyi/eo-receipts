using System.Text;
using System.Text.Json;

namespace Receipts.Claude;

/// <summary>
/// Text of a Messages API response.
/// </summary>
public sealed class ClaudeReply
{
    private readonly JsonElement reply;

    public ClaudeReply(JsonElement reply)
    {
        this.reply = reply;
    }

    public string Text()
    {
        var stop = this.reply.TryGetProperty("stop_reason", out var reason) ? reason.GetString() : string.Empty;
        if (stop == "refusal")
        {
            throw new LlmException("claude", 200, "The model refused to answer");
        }
        if (stop == "max_tokens")
        {
            throw new MalformedAnswerException("claude: the answer was cut by max_tokens");
        }
        var text = new StringBuilder();
        if (this.reply.TryGetProperty("content", out var blocks) && blocks.ValueKind == JsonValueKind.Array)
        {
            foreach (var block in blocks.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var type) && type.GetString() == "text")
                {
                    text.Append(block.GetProperty("text").GetString());
                }
            }
        }
        if (text.Length == 0)
        {
            throw new MalformedAnswerException("claude: the answer has no text");
        }
        return text.ToString();
    }
}
