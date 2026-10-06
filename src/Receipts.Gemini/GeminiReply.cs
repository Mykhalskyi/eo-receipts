using System.Text;
using System.Text.Json;

namespace Receipts.Gemini;

/// <summary>
/// Text of a generateContent response.
/// </summary>
public sealed class GeminiReply
{
    private readonly JsonElement reply;

    public GeminiReply(JsonElement reply)
    {
        this.reply = reply;
    }

    public string Text()
    {
        if (this.reply.TryGetProperty("promptFeedback", out var feedback) && feedback.TryGetProperty("blockReason", out var blocked))
        {
            throw new LlmException("gemini", 200, $"The prompt was blocked: {blocked.GetString()}");
        }
        if (!this.reply.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            throw new MalformedAnswerException("gemini: the answer has no candidates");
        }
        var candidate = candidates[0];
        var finish = candidate.TryGetProperty("finishReason", out var reason) ? reason.GetString() : "STOP";
        if (finish == "MAX_TOKENS")
        {
            throw new MalformedAnswerException("gemini: the answer was cut by the token limit");
        }
        if (finish is "SAFETY" or "RECITATION" or "BLOCKLIST" or "PROHIBITED_CONTENT" or "SPII")
        {
            throw new LlmException("gemini", 200, $"The answer was blocked: {finish}");
        }
        var text = new StringBuilder();
        if (candidate.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                var thought = part.TryGetProperty("thought", out var flag) && flag.ValueKind == JsonValueKind.True;
                if (!thought && part.TryGetProperty("text", out var piece))
                {
                    text.Append(piece.GetString());
                }
            }
        }
        if (text.Length == 0)
        {
            throw new MalformedAnswerException("gemini: the answer has no text");
        }
        return text.ToString();
    }
}
