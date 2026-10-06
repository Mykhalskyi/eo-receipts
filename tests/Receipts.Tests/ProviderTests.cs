using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Receipts.Claude;
using Receipts.Gemini;
using Receipts.Ollama;
using Receipts.OpenAi;

namespace Receipts.Tests;

public sealed class ProviderTests
{
    private readonly string answer = JsonSerializer.Serialize(new CafeJson().ToString());

    [Fact]
    public async Task CallsClaude()
    {
        var stub = new StubHttp($$"""{"content": [{"type": "text", "text": {{this.answer}}}], "stop_reason": "end_turn"}""");
        var llm = new ClaudeLlm(new TextCredential("secret"), "claude-opus-5-5", new HttpClient(stub));
        var receipt = await new LlmReceipts(llm).Receipt(new TinyPng().Image());
        Assert.Equal(8.2m, receipt.Total().Amount());
        Assert.Equal("https://api.anthropic.com/v1/messages", stub.Uri!.ToString());
        Assert.Equal("secret", stub.Headers["x-api-key"]);
        Assert.Equal("2023-06-01", stub.Headers["anthropic-version"]);
        var body = JsonNode.Parse(stub.Body)!;
        Assert.Equal("claude-opus-5-5", body["model"]!.GetValue<string>());
        Assert.Equal("json_schema", body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Equal("object", body["output_config"]!["format"]!["schema"]!["type"]!.GetValue<string>());
        var image = body["messages"]![0]!["content"]![0]!;
        Assert.Equal("image", image["type"]!.GetValue<string>());
        Assert.Equal("image/png", image["source"]!["media_type"]!.GetValue<string>());
        Assert.Equal("iVBORw0KGgoBAgM=", image["source"]!["data"]!.GetValue<string>());
        Assert.Equal("text", body["messages"]![0]!["content"]![1]!["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task ReportsClaudeRefusal()
    {
        var stub = new StubHttp("""{"content": [], "stop_reason": "refusal"}""");
        var llm = new ClaudeLlm(new TextCredential("secret"), "claude-opus-5-5", new HttpClient(stub));
        var error = await Assert.ThrowsAsync<LlmException>(() => llm.Answer(new ReceiptPrompt(new TinyPng().Image())));
        Assert.False(error.Transient);
    }

    [Fact]
    public async Task ReportsOverloadAsTransient()
    {
        var stub = new StubHttp((HttpStatusCode)529, """{"type": "error", "error": {"type": "overloaded_error"}}""");
        var llm = new ClaudeLlm(new TextCredential("secret"), "claude-opus-5-5", new HttpClient(stub));
        var error = await Assert.ThrowsAsync<LlmException>(() => llm.Answer(new ReceiptPrompt(new TinyPng().Image())));
        Assert.Equal(529, error.Status);
        Assert.True(error.Transient);
    }

    [Fact]
    public async Task CallsOpenAi()
    {
        var stub = new StubHttp($$"""{"choices": [{"message": {"content": {{this.answer}}, "refusal": null}, "finish_reason": "stop"}]}""");
        var llm = new OpenAiLlm(new TextCredential("sk"), "gpt-5", new HttpClient(stub));
        var receipt = await new LlmReceipts(llm).Receipt(new TinyPng().Image());
        Assert.Equal(8.2m, receipt.Total().Amount());
        Assert.Equal("https://api.openai.com/v1/chat/completions", stub.Uri!.ToString());
        Assert.Equal("Bearer sk", stub.Headers["Authorization"]);
        var body = JsonNode.Parse(stub.Body)!;
        Assert.True(body["response_format"]!["json_schema"]!["strict"]!.GetValue<bool>());
        Assert.Equal(
            "data:image/png;base64,iVBORw0KGgoBAgM=",
            body["messages"]![0]!["content"]![0]!["image_url"]!["url"]!.GetValue<string>()
        );
    }

    [Fact]
    public async Task ReportsOpenAiRefusal()
    {
        var stub = new StubHttp("""{"choices": [{"message": {"content": null, "refusal": "I can't"}, "finish_reason": "stop"}]}""");
        var llm = new OpenAiLlm(new TextCredential("sk"), "gpt-5", new HttpClient(stub));
        await Assert.ThrowsAsync<LlmException>(() => llm.Answer(new ReceiptPrompt(new TinyPng().Image())));
    }

    [Fact]
    public async Task CallsAzureOpenAi()
    {
        var stub = new StubHttp($$"""{"choices": [{"message": {"content": {{this.answer}}}, "finish_reason": "stop"}]}""");
        var llm = new AzureOpenAiLlm(new Uri("https://acme.openai.azure.com"), new TextCredential("az"), "receipts", new HttpClient(stub));
        await llm.Answer(new ReceiptPrompt(new TinyPng().Image()));
        Assert.Equal(
            "https://acme.openai.azure.com/openai/deployments/receipts/chat/completions?api-version=2024-10-21",
            stub.Uri!.ToString()
        );
        Assert.Equal("az", stub.Headers["api-key"]);
        Assert.False(stub.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task CallsOpenAiCompatibleServerWithoutKey()
    {
        var stub = new StubHttp($$"""{"choices": [{"message": {"content": {{this.answer}}}, "finish_reason": "stop"}]}""");
        var llm = new OpenAiCompatibleLlm(new Uri("http://localhost:1234/v1"), "qwen2.5-vl-7b", new HttpClient(stub));
        await llm.Answer(new ReceiptPrompt(new TinyPng().Image()));
        Assert.Equal("http://localhost:1234/v1/chat/completions", stub.Uri!.ToString());
        Assert.False(stub.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task CallsGemini()
    {
        var stub = new StubHttp(
            $$"""{"candidates": [{"content": {"parts": [{"text": {{this.answer}}}], "role": "model"}, "finishReason": "STOP"}]}"""
        );
        var llm = new GeminiLlm(new TextCredential("g"), "gemini-2.5-flash", new HttpClient(stub));
        var receipt = await new LlmReceipts(llm).Receipt(new TinyPng().Image());
        Assert.Equal(8.2m, receipt.Total().Amount());
        Assert.Equal(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
            stub.Uri!.ToString()
        );
        Assert.Equal("g", stub.Headers["x-goog-api-key"]);
        var body = JsonNode.Parse(stub.Body)!;
        Assert.Equal("application/json", body["generationConfig"]!["responseMimeType"]!.GetValue<string>());
        Assert.NotNull(body["generationConfig"]!["responseJsonSchema"]);
        Assert.Equal("image/png", body["contents"]![0]!["parts"]![0]!["inline_data"]!["mime_type"]!.GetValue<string>());
    }

    [Fact]
    public async Task ReportsGeminiBlock()
    {
        var stub = new StubHttp("""{"promptFeedback": {"blockReason": "SAFETY"}}""");
        var llm = new GeminiLlm(new TextCredential("g"), "gemini-2.5-flash", new HttpClient(stub));
        await Assert.ThrowsAsync<LlmException>(() => llm.Answer(new ReceiptPrompt(new TinyPng().Image())));
    }

    [Fact]
    public async Task CallsOllama()
    {
        var stub = new StubHttp($$"""{"message": {"role": "assistant", "content": {{this.answer}}}, "done": true, "done_reason": "stop"}""");
        var llm = new OllamaLlm(new Uri("http://gpu-box:11434/"), "qwen2.5vl:7b", new HttpClient(stub));
        var receipt = await new LlmReceipts(llm).Receipt(new TinyPng().Image());
        Assert.Equal(8.2m, receipt.Total().Amount());
        Assert.Equal("http://gpu-box:11434/api/chat", stub.Uri!.ToString());
        var body = JsonNode.Parse(stub.Body)!;
        Assert.False(body["stream"]!.GetValue<bool>());
        Assert.Equal("object", body["format"]!["type"]!.GetValue<string>());
        Assert.Equal("iVBORw0KGgoBAgM=", body["messages"]![0]!["images"]![0]!.GetValue<string>());
        Assert.Contains("Extract the data", body["messages"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    public async Task ReportsNetworkFailureAsTransient()
    {
        var llm = new OllamaLlm(new Uri("http://127.0.0.1:1"), "llava", new HttpClient());
        var error = await Assert.ThrowsAsync<LlmException>(() => llm.Answer(new ReceiptPrompt(new TinyPng().Image())));
        Assert.Equal(0, error.Status);
        Assert.True(error.Transient);
    }

    [Fact]
    public async Task RequiresEnvironmentCredential()
    {
        await Assert.ThrowsAsync<LlmException>(() => new EnvCredential($"RECEIPTS_MISSING_{Guid.NewGuid():N}").Value());
    }
}
