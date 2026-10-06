# EO Receipts — AI receipt processing library for C# (design)

## Context
Goal: a C# library that turns a receipt **image** into **structured data** using an LLM.
It has to support several image formats and several LLMs, both cloud and local. It has to be
strictly object-oriented and follow Elegant Objects (EO). This document covers design only:
types, implementations and usage. No code is written yet.

### EO rules applied
| Rule | How the design follows it |
|---|---|
| No `-er`/`-or` classes (Parser, Processor, Manager) | Objects are named after the things they are: `Receipt`, `JpegImage`, `ClaudeLlm`, `LlmReceipts` |
| Everything immutable | All classes are `sealed`, fields are `readonly`, and there are no setters |
| Constructors only assign | No I/O or parsing in constructors. Work happens lazily in methods |
| One primary constructor, others delegate | Secondary constructors use `: this(...)` |
| No `null` | Absent data comes back as an empty collection or a Null Object, or the call throws. Nothing returns `null` |
| No static methods or utility classes | Behaviour lives in small objects, and the user composes them |
| Decorators instead of inheritance | Retry, cache, fallback, resize and validation are all decorators |
| Small interfaces, one per concept | Every public type is an interface with 1–4 methods |
| No type casting or reflection | Format and provider differences are handled by polymorphism |
| No DI container required | Users wire objects together with `new` in their composition root |

---

## 1. Packages
```
Receipts.Core        interfaces, receipt model, prompt, JSON reading, decorators
Receipts.Images      format detection + conversion/resizing (SixLabors.ImageSharp)
Receipts.Claude      Anthropic Messages API
Receipts.OpenAi      OpenAI + any OpenAI-compatible server (Azure, LM Studio, vLLM, llama.cpp)
Receipts.Gemini      Google Gemini
Receipts.Ollama      local Ollama (qwen2.5-vl, llama3.2-vision, llava, …)
Receipts.Testing     fakes for unit tests
```
Core has no dependency on any provider SDK. Each provider package talks plain HTTP through an
injected `HttpClient`.

---

## 2. Bytes (where an image comes from)
```csharp
public interface IContent
{
    Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default);
}
```
Implementations: `FileContent(string path)`, `StreamContent(Stream)`,
`UrlContent(Uri, HttpClient)`, `Base64Content(string)`, `ArrayContent(byte[])`.
There is also a `StickyContent(IContent)` decorator that reads once and then caches.

## 3. Images (many formats)
```csharp
public interface IImage
{
    Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default);
    IMediaType Type();                                  // image/jpeg, image/png, …
}
public interface IMediaType { string Mime(); }          // MediaType("image/png")
```
**Format implementations** (each wraps an `IContent`):
- `JpegImage`, `PngImage`, `WebpImage`, `GifImage`, `HeicImage`, `TiffImage`, `BmpImage`
- `PdfPageImage(IContent pdf, int page)` for scanned receipts that arrive as PDF
- `DetectedImage(IContent)` reads magic bytes lazily, picks the format, and throws
  `UnsupportedImageException` for anything unknown. This is the usual entry point.

**Decorators** (in `Receipts.Images`):
- `JpegOf(IImage)` re-encodes the image to JPEG. Use it for formats a model doesn't accept,
  such as HEIC, TIFF or BMP.
- `ResizedImage(IImage, int maxSide)` scales the image down to cut tokens and cost.
- `GrayscaleImage(IImage)`, `RotatedImage(IImage, int degrees)` and
  `AutoOrientedImage(IImage)` (EXIF) are pre-processing steps.
- `StickyImage(IImage)` keeps the result after the first evaluation.

Each LLM also states which formats it accepts:
```csharp
public interface IAcceptedTypes { bool Contains(IMediaType type); }
```
`CompatibleImage(IImage, IAcceptedTypes)` passes the original image through unchanged when the
model accepts its format. Otherwise it converts the image with `JpegOf`.

## 4. Prompt and schema
```csharp
public interface IPrompt
{
    Task<IReadOnlyList<IPart>> Parts(CancellationToken ct = default); // text + image parts
    IJsonSchema Schema();                                             // expected output shape
}
public interface IPart { void Print(IPartMedia media); }  // EO "printer" — no getters/casts
public interface IPartMedia { void Text(string text); void Image(IImage image); }
public interface IJsonSchema { string Json(); }
```
- `TextPart(string)`, `ImagePart(IImage)`
- `ReceiptPrompt(IImage)` is the default extraction instructions plus `ReceiptSchema`.
  A secondary constructor `ReceiptPrompt(IImage, string instructions)` takes custom wording.
- `ReceiptSchema` is the JSON Schema for the model in section 7: merchant, date, currency,
  lines, taxes, totals and payment.
- `LocalizedPrompt(IPrompt, CultureInfo)` adds a hint about language and number format.

Each provider implements `IPartMedia` to write its own wire format. Claude uses content blocks,
OpenAI uses `image_url`, Gemini uses `inline_data`, and Ollama uses `images[]`. This replaces
the `if (part is ImagePart)` casting the design would otherwise need.

## 5. LLMs (cloud and local)
```csharp
public interface ILlm
{
    Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default);
    IAcceptedTypes Accepts();
}
public interface IJson { JsonElement Root(); }           // parsed lazily
```
Each provider maps `IPrompt.Schema()` to its own structured-output feature, so the model
returns valid JSON instead of free text.

| Class | Kind | Structured output via |
|---|---|---|
| `ClaudeLlm(ICredential key, string model, HttpClient)` | cloud | structured outputs / forced tool use |
| `OpenAiLlm(ICredential key, string model, HttpClient)` | cloud | `response_format: json_schema` |
| `AzureOpenAiLlm(Uri endpoint, ICredential, string deployment, HttpClient)` | cloud | same |
| `GeminiLlm(ICredential key, string model, HttpClient)` | cloud | `responseSchema` |
| `OllamaLlm(Uri host, string model, HttpClient)` | local | `format: <schema>` |
| `OpenAiCompatibleLlm(Uri baseUrl, string model, HttpClient)` | local | LM Studio / vLLM / llama.cpp server |

`ICredential` can be `EnvCredential("ANTHROPIC_API_KEY")`, `TextCredential("...")` or
`AzureKeyVaultCredential(...)`. Keys are read lazily, never in a constructor.

**LLM decorators**, which all take `ILlm` and return `ILlm`:
- `RetryingLlm(ILlm, int attempts, TimeSpan backoff)` retries transient HTTP failures and
  invalid JSON.
- `TimedLlm(ILlm, TimeSpan timeout)`
- `FallbackLlm(ILlm primary, ILlm secondary)` is useful for "local first, cloud if it fails".
- `ThrottledLlm(ILlm, int maxConcurrent)` limits concurrency to respect rate limits.
- `LoggedLlm(ILlm, ILogger)`
- `CachedLlm(ILlm, ICache)` caches by a hash of the prompt and image bytes, so the same image
  is never paid for twice.
- `FencedJsonLlm(ILlm)` strips ```` ```json ```` fences and leading prose. Use it with weak
  local models that lack schema support.

## 6. Receipts (collection) and receipt (entity)
EO favours a collection object as the factory for entities, so the library has `Receipts`, not
a `ReceiptProcessor`.
```csharp
public interface IReceipts
{
    Task<IReceipt> Receipt(IImage image, CancellationToken ct = default);
}
```
- `LlmReceipts(ILlm llm)` delegates to `LlmReceipts(llm, image => new ReceiptPrompt(image))`.
  It builds `CompatibleImage`, asks the LLM, and returns `JsonReceipt(json)`.
- `ValidReceipts(IReceipts)` wraps each result in a `ValidReceipt`.
- `VotedReceipts(IReadOnlyList<IReceipts>)` asks several models, keeps the fields where most
  of them agree, and flags disagreements. This is optional and aimed at high accuracy.

## 7. Structured result model
All of these are interfaces with JSON-backed implementations. They read lazily from the
`JsonElement`, and nothing is parsed in a constructor.
```csharp
public interface IReceipt
{
    IMerchant Merchant();
    DateTimeOffset Date();                  // throws MissingFieldException if the model found none
    IReadOnlyList<ILine> Lines();           // empty, never null
    IReadOnlyList<ITax> Taxes();
    IMoney Subtotal();
    IMoney Total();
    IPayment Payment();                     // NoPayment Null Object when absent
    void Print(IReceiptMedia media);        // export without getters leaking structure
}
public interface IMerchant { string Name(); string Address(); string TaxId(); } // "" if unknown
public interface ILine { string Name(); decimal Quantity(); IMoney UnitPrice(); IMoney Total(); }
public interface ITax  { string Name(); decimal Rate(); IMoney Amount(); }
public interface IPayment { string Method(); string CardLast4(); }
public interface IMoney { decimal Amount(); ICurrency Currency(); IMoney Plus(IMoney other); }
public interface ICurrency { string Code(); }            // ISO 4217
```
Implementations: `JsonReceipt(IJson)`, `JsonLine(JsonElement)`, `JsonMerchant`,
`Money(decimal, ICurrency)`, `Currency("EUR")`, `NoPayment`.

**Receipt decorators:**
- `ValidReceipt(IReceipt, decimal tolerance)` checks on access that the lines add up to the
  subtotal, that subtotal plus taxes equals the total, and that the date is not in the future.
  It throws `InvalidReceiptException` and lists every violation.
- `StickyReceipt(IReceipt)` memoises values that are expensive to compute.

**Export uses printers instead of a DTO full of getters:**
```csharp
public interface IReceiptMedia { /* With(string key, object value), nested sections */ }
```
`JsonMedia`, `CsvMedia` (one row per line), and `RecordMedia` (which fills a plain C# `record`
for EF Core or ASP.NET interop).

## 8. Exceptions (no null, no error codes)
`ReceiptException` is the base. It has these subclasses: `UnsupportedImageException`,
`LlmException` (carrying the HTTP status and provider), `MalformedAnswerException`,
`MissingFieldException` and `InvalidReceiptException`.

---

## 9. Usage examples

**Simplest cloud case**
```csharp
IReceipts receipts = new LlmReceipts(
    new ClaudeLlm(new EnvCredential("ANTHROPIC_API_KEY"), "claude-sonnet-5-5", http));

IReceipt receipt = await receipts.Receipt(
    new DetectedImage(new FileContent("lunch.heic")));    // HEIC → auto-converted if needed

Console.WriteLine($"{receipt.Merchant().Name()}: {receipt.Total().Amount()} {receipt.Total().Currency().Code()}");
foreach (ILine line in receipt.Lines())
    Console.WriteLine($"  {line.Quantity()} × {line.Name()} = {line.Total().Amount()}");
```

**Fully local (no data leaves the machine)**
```csharp
IReceipts receipts = new LlmReceipts(
    new FencedJsonLlm(
        new OllamaLlm(new Uri("http://localhost:11434"), "qwen2.5-vl:7b", http)));
```

**Production composition: local first, cloud fallback, retries, caching and validation**
```csharp
IReceipts receipts =
    new ValidReceipts(
        new LlmReceipts(
            new CachedLlm(
                new FallbackLlm(
                    new TimedLlm(new OllamaLlm(ollamaUri, "llama3.2-vision", http), TimeSpan.FromSeconds(30)),
                    new RetryingLlm(
                        new ThrottledLlm(
                            new OpenAiLlm(new EnvCredential("OPENAI_API_KEY"), "gpt-5", http), 4),
                        3, TimeSpan.FromSeconds(2))),
                new FileCache("./.receipt-cache"))));
```

**Pre-processing and other sources**
```csharp
IImage image =
    new ResizedImage(
        new AutoOrientedImage(
            new DetectedImage(new UrlContent(new Uri("https://…/scan.png"), http))),
        1600);
```

**Batch processing**
```csharp
IReceipt[] all = await Task.WhenAll(
    Directory.EnumerateFiles("inbox").Select(f =>
        receipts.Receipt(new DetectedImage(new FileContent(f)))));
```

**Export**
```csharp
var json = new JsonMedia();
receipt.Print(json);
await File.WriteAllTextAsync("lunch.json", json.ToString());
```

**Unit testing without a network**
```csharp
IReceipts receipts = new LlmReceipts(new FakeLlm(new JsonOf("{\"total\":{\"amount\":12.5,\"currency\":\"EUR\"}, …}")));
```

**Adding a new provider** means implementing `ILlm` and `IPartMedia` (two small classes). No
existing class changes.

**Adding a new format** means implementing `IImage` (for example `AvifImage`) and registering a
signature in `DetectedImage`'s list of candidates. That list is passed in through a secondary
constructor, so users can extend it without forking the library.

---

## 10. Verification (once implemented)
- Unit tests in `Receipts.Core.Tests` use `FakeLlm` and `ArrayContent`. They cover JSON → model
  mapping, `ValidReceipt` arithmetic, every decorator, and `DetectedImage` on sample headers for
  each format.
- Contract tests per provider replay recorded HTTP fixtures (`HttpMessageHandler` stub) and
  check the request shape (image encoding, schema field) and how the response is parsed.
- A golden-set integration test runs about 20 real receipt images in different formats,
  languages and currencies with known expected JSON. It runs against each configured LLM, cloud
  ones behind env-var keys and Ollama in CI through a container, and reports field-level
  accuracy.
- An EO lint (Roslyn analyzer or architecture test with NetArchTest) checks that every class is
  `sealed`, there are no `static` members except constants, no public setters, and no class
  names ending in `-er`/`-or`.
