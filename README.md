# EO Receipts

A C# library for AI receipt processing: a receipt **image** goes in and **structured data** comes out.
It reads many image formats and works with cloud models (Claude, OpenAI, Azure OpenAI, Gemini) and
local ones (Ollama, LM Studio, vLLM, llama.cpp). The design is strictly object-oriented and follows
[Elegant Objects](https://www.elegantobjects.org/).

```csharp
IReceipts receipts = new LlmReceipts(
    new CompatibleLlm(
        new ClaudeLlm(new EnvCredential("ANTHROPIC_API_KEY"), "claude-opus-5-5", http)));

IReceipt receipt = await receipts.Receipt(new DetectedImage(new FileContent("lunch.heic")));

Console.WriteLine($"{receipt.Merchant().Name()}: {receipt.Total()}");
foreach (ILine line in receipt.Lines())
    Console.WriteLine($"  {line.Quantity()} × {line.Name()} = {line.Total().Amount()}");
```

## Packages

| Project | Contents | Dependencies |
|---|---|---|
| `Receipts.Core` | interfaces, receipt model, prompt and schema, image sources and format detection, LLM decorators, export | `Microsoft.Extensions.Logging.Abstractions` |
| `Receipts.Images` | conversion to JPEG, resizing, rotation, grayscale, EXIF orientation, PDF pages, `CompatibleLlm` | `Magick.NET-Q8-AnyCPU` |
| `Receipts.Claude` | Anthropic Messages API | Core |
| `Receipts.OpenAi` | OpenAI, Azure OpenAI and OpenAI-compatible servers | Core |
| `Receipts.Gemini` | Google Gemini API | Core |
| `Receipts.Ollama` | local Ollama server | Core |
| `Receipts.Testing` | `FakeLlm`, `BrokenLlm`, `ScriptedLlm` for unit tests | Core |

The provider packages call the HTTP APIs directly through an injected `HttpClient`. Core has no provider SDK
dependency, and every provider can be tested with a stub `HttpMessageHandler`.

## Elegant Objects rules

| Rule | How the library follows it |
|---|---|
| No `-er`/`-or` names (Parser, Processor, Manager) | Objects are named after what they are: `JsonReceipt`, `JpegOf`, `ClaudeLlm`, `LlmReceipts` |
| Immutable objects | Every class is `sealed`, fields are `readonly`, there are no setters. Even media objects return a new copy from `With(...)` |
| Constructors only assign | No I/O or parsing in constructors. Work happens lazily in methods |
| One primary constructor | Secondary constructors delegate with `: this(...)` |
| No `null` | Absent data is an empty string or collection, a Null Object (`NoPayment`), or an exception (`AbsentFieldException`) |
| No static methods | All behaviour lives in small objects composed with `new` |
| Decorators, not inheritance | Retry, timeout, fallback, cache, throttling, logging, validation and image conversions are all decorators |
| Printers instead of getters | Prompt parts print into provider-specific media, and receipts print into JSON, CSV or a record |
| No type casting | Format and provider differences are handled by polymorphism |

`ElegantObjectsTests` enforces these rules on the public API by reflection: classes are sealed, there are no
public static methods and no properties, and no names end in `-er` or `-or`. Exceptions and the
`*Record` DTOs are the deliberate exceptions.

## Types

### Image sources: `IContent`
```csharp
public interface IContent { Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default); }
```
`FileContent(path)`, `StreamedContent(Stream)`, `UrlContent(Uri, HttpClient)`, `Base64Content(string)`,
`ArrayContent(byte[])`, and the `StickyContent(IContent)` decorator, which reads once.

### Images: `IImage`
```csharp
public interface IImage
{
    Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default);
    Task<IMediaType> Type(CancellationToken ct = default);
}
```
- **Known formats:** `JpegImage`, `PngImage`, `WebpImage`, `GifImage`, `HeicImage`, `TiffImage`, `BmpImage`,
  `TypedImage(IContent, mime)`.
- **`DetectedImage(IContent)`** detects the format from magic bytes and throws `UnsupportedImageException` for
  unknown ones. To recognise more formats, pass your own signatures:
  `new DetectedImage(content, [..new KnownSignatures(), new Signature("image/avif", 4, "ftypavif")])`.
- **`StickyImage(IImage)`** caches an expensive conversion.
- **`Receipts.Images` decorators:** `JpegOf`, `ResizedImage(image, maxSide)`, `GrayscaleImage`, `RotatedImage(image, degrees)`,
  `AutoOrientedImage` (EXIF), `CompatibleImage(image, IAcceptedTypes)`, and
  `PdfPageImage(IContent pdf, page)`, which needs Ghostscript installed.

### Prompt: `IPrompt`
```csharp
public interface IPrompt { IReadOnlyList<IPart> Parts(); IJsonSchema Schema(); }
public interface IPart { Task<IPartMedia<T>> Print<T>(IPartMedia<T> media, CancellationToken ct = default); }
public interface IPartMedia<T>
{
    Task<IPartMedia<T>> Text(string text, CancellationToken ct = default);
    Task<IPartMedia<T>> Image(IImage image, CancellationToken ct = default);
    T Value();
}
```
- `ReceiptPrompt(image)` is the image plus the default extraction instructions, answered with `ReceiptSchema`.
  `ReceiptPrompt(image, instructions)` takes your own wording.
- `LocalizedPrompt(prompt, CultureInfo)` adds a hint about the language and the number and date formats.
- Each provider prints parts in its own wire format (`ClaudeFormat`, `OpenAiFormat`, `GeminiFormat`,
  `OllamaMessage`), so there is no `if (part is ImagePart)`.

### Models: `ILlm`
```csharp
public interface ILlm
{
    Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default);
    IAcceptedTypes Accepts();
}
```

| Class | Where | Structured output |
|---|---|---|
| `ClaudeLlm(ICredential, model, HttpClient)` | cloud | `output_config.format` with JSON schema |
| `OpenAiLlm(ICredential, model, HttpClient)` | cloud | `response_format: json_schema` (strict) |
| `AzureOpenAiLlm(Uri endpoint, ICredential, deployment, HttpClient)` | cloud | same |
| `GeminiLlm(ICredential, model, HttpClient)` | cloud | `responseJsonSchema` |
| `OllamaLlm(Uri host, model, HttpClient)` | local | `format: <schema>` |
| `OpenAiCompatibleLlm(Uri root, model, HttpClient)` | local | `response_format` (LM Studio, vLLM, llama.cpp server) |

Credentials: `EnvCredential("ANTHROPIC_API_KEY")`, `TextCredential("...")`, `NoCredential`. A credential is read
only when a request is sent.

Decorators (`ILlm` → `ILlm`):

| Decorator | Purpose |
|---|---|
| `RetryingLlm(llm, attempts, backoff)` | retries transient failures (network, timeout, 408/409/429/5xx) and non-JSON answers with exponential backoff |
| `TimedLlm(llm, timeout)` | gives up after the timeout with a transient `LlmException` |
| `FallbackLlm(primary, secondary)` | e.g. a local model first and a cloud one if it fails |
| `ThrottledLlm(llm, concurrency)` | limits concurrent calls |
| `LoggedLlm(llm, ILogger)` | logs the duration and outcome of each call |
| `CachedLlm(llm, ICache)` | caches answers by a SHA-256 of the schema, texts and image bytes. `InMemoryCache`, `FileCache(dir)` |
| `FencedJsonLlm(llm)` | strips Markdown fences and prose for models without schema support |
| `CompatibleLlm(llm)` (Images) | converts images the model can't read to JPEG |

### Receipts: `IReceipts` and `IReceipt`
```csharp
public interface IReceipts { Task<IReceipt> Receipt(IImage image, CancellationToken ct = default); }

public interface IReceipt : IPrintable
{
    IMerchant Merchant();            // Name(), Address(), TaxId(), "" when unknown
    DateTimeOffset Date();           // AbsentFieldException when not on the receipt
    IReadOnlyList<ILine> Lines();    // Name(), Quantity(), UnitPrice(), Total()
    IReadOnlyList<ITax> Taxes();     // Name(), Rate(), Amount()
    IMoney Subtotal();               // printed, or sum of lines, or total minus taxes
    IMoney Total();
    IPayment Payment();              // Method(), CardLast4(); NoPayment when absent
}
```
- `LlmReceipts(llm)` or `LlmReceipts(llm, image => prompt)` asks a model and returns a lazily read `JsonReceipt`.
- `ValidReceipts(receipts, tolerance)` / `ValidReceipt(receipt)` check that each line equals quantity × unit
  price, that the lines add up to the subtotal (or to the total for tax-inclusive prices), that the subtotal
  plus taxes equals the total, and that the date is not in the future. They throw `InvalidReceiptException`
  listing every violation.
- `VotedReceipts(receipts...)` asks several models at once and returns a receipt whose total is confirmed by a
  majority.
- `StickyReceipt(receipt)` memoises values.

### Export: printers
```csharp
JsonObject json   = receipt.Print(new JsonMedia()).Value();
string csv        = receipt.Print(new CsvMedia()).Value();      // one row per line
ReceiptRecord dto = receipt.Print(new RecordMedia()).Value();   // plain C# record for EF Core / ASP.NET
```

### Exceptions
`ReceiptException` is the base class. The subclasses are `UnsupportedImageException`, `LlmException`
(`Provider`, `Status`, `Transient`), `MalformedAnswerException`, `AbsentFieldException` and
`InvalidReceiptException` (`Violations`).

## Usage

**Fully local, no data leaves the machine**
```csharp
IReceipts receipts = new LlmReceipts(
    new CompatibleLlm(
        new OllamaLlm(new Uri("http://localhost:11434"), "qwen2.5vl:7b", http)));
```

**Production composition: local first, cloud as a fallback, with retries, throttling, caching and validation**
```csharp
IReceipts receipts =
    new ValidReceipts(
        new LlmReceipts(
            new CompatibleLlm(
                new CachedLlm(
                    new FallbackLlm(
                        new TimedLlm(new OllamaLlm(ollama, "llama3.2-vision", http), TimeSpan.FromSeconds(60)),
                        new RetryingLlm(
                            new ThrottledLlm(
                                new OpenAiLlm(new EnvCredential("OPENAI_API_KEY"), "gpt-5", http), 4))),
                    new FileCache("./.receipt-cache")))));
```

**Pre-processing and other sources**
```csharp
IImage image =
    new ResizedImage(
        new AutoOrientedImage(
            new DetectedImage(new UrlContent(new Uri("https://example.com/scan.png"), http))),
        1600);

IImage scanned = new PdfPageImage(new FileContent("invoice.pdf"), 1);
```

**Several models voting**
```csharp
IReceipts receipts = new VotedReceipts(
    new LlmReceipts(new CompatibleLlm(new ClaudeLlm(new EnvCredential("ANTHROPIC_API_KEY"), http))),
    new LlmReceipts(new CompatibleLlm(new GeminiLlm(new EnvCredential("GEMINI_API_KEY"), "gemini-2.5-flash", http))),
    new LlmReceipts(new CompatibleLlm(new OllamaLlm("qwen2.5vl:7b", http))));
```

**Batch processing**
```csharp
IReceipt[] all = await Task.WhenAll(
    Directory.EnumerateFiles("inbox").Select(file =>
        receipts.Receipt(new DetectedImage(new FileContent(file)))));
```

**Unit testing without a network**
```csharp
var llm = new FakeLlm("""{"total": 12.5, "currency": "EUR"}""");
IReceipt receipt = await new LlmReceipts(llm).Receipt(new PngImage(new ArrayContent(bytes)));
```

**Extending:** a new provider is one `ILlm` class plus an `IPartFormat` (or `IPartMedia<T>`). A new image format
is an `IImage` or an `ISignature` passed to `DetectedImage`. Neither requires changes to existing classes.

## Building and testing

The projects target .NET 8.

```bash
dotnet build Receipts.sln
dotnet test Receipts.sln
```

The tests cover format detection, JSON → model mapping, validation, every decorator, export media and Magick.NET
conversions. They also include contract tests for each provider that check the HTTP request (URL, headers,
schema, image encoding) and how the reply is parsed against a stub `HttpMessageHandler`, plus the EO
architecture checks. None of them call a real model.
