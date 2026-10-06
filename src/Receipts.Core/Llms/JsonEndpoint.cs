using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// HTTP endpoint accepting and returning JSON. Turns HTTP and network failures into <see cref="LlmException"/>.
/// </summary>
public sealed class JsonEndpoint
{
    private readonly HttpClient http;
    private readonly Uri uri;
    private readonly string provider;
    private readonly IReadOnlyDictionary<string, string> headers;

    public JsonEndpoint(HttpClient http, Uri uri, string provider) : this(http, uri, provider, new Dictionary<string, string>())
    {
    }

    public JsonEndpoint(HttpClient http, Uri uri, string provider, IReadOnlyDictionary<string, string> headers)
    {
        this.http = http;
        this.uri = uri;
        this.provider = provider;
        this.headers = headers;
    }

    public async Task<JsonElement> Reply(JsonObject body, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, this.uri);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        foreach (var header in this.headers.Where(header => header.Value.Length > 0))
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        HttpResponseMessage response;
        try
        {
            response = await this.http.SendAsync(request, ct);
        }
        catch (HttpRequestException e)
        {
            throw new LlmException(this.provider, 0, e.Message, e);
        }
        catch (TaskCanceledException e) when (!ct.IsCancellationRequested)
        {
            throw new LlmException(this.provider, 408, "The request timed out", e);
        }
        using (response)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new LlmException(this.provider, (int)response.StatusCode, new Excerpt(text, 1000).ToString());
            }
            try
            {
                using var document = JsonDocument.Parse(text);
                return document.RootElement.Clone();
            }
            catch (JsonException e)
            {
                throw new MalformedAnswerException($"{this.provider} replied with non-JSON: {new Excerpt(text)}", e);
            }
        }
    }
}
