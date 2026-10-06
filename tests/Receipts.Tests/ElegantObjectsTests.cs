using System.Reflection;
using System.Runtime.CompilerServices;
using Receipts.Claude;
using Receipts.Gemini;
using Receipts.Images;
using Receipts.Ollama;
using Receipts.OpenAi;
using Receipts.Testing;

namespace Receipts.Tests;

/// <summary>
/// Keeps the public API in line with Elegant Objects.
/// Exceptions and data records are the deliberate exceptions to the rules.
/// </summary>
public sealed class ElegantObjectsTests
{
    private readonly IReadOnlyList<Type> types =
    [
        .. new[]
            {
                typeof(IReceipt), typeof(ClaudeLlm), typeof(OpenAiLlm), typeof(GeminiLlm),
                typeof(OllamaLlm), typeof(JpegOf), typeof(FakeLlm),
            }
            .SelectMany(type => type.Assembly.GetExportedTypes())
            .Where(type => !typeof(Exception).IsAssignableFrom(type))
            .Where(type => type.GetMethod("<Clone>$") is null),
    ];

    [Fact]
    public void ClassesAreSealed()
    {
        Assert.True(this.types.Count > 80, $"Only {this.types.Count} public types found");
        Assert.Empty(this.types.Where(type => type.IsClass && !type.IsSealed).Select(type => type.FullName));
    }

    [Fact]
    public void ThereAreNoPublicStaticMethods()
    {
        Assert.Empty(
            this.types
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Select(method => $"{method.DeclaringType!.Name}.{method.Name}")
        );
    }

    [Fact]
    public void ThereAreNoProperties()
    {
        Assert.Empty(
            this.types
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")
        );
    }

    [Fact]
    public void NamesDoNotEndWithErOrOr()
    {
        var agents = this.types
            .Select(type => type.Name.Split('`')[0])
            .Where(name => name.EndsWith("er", StringComparison.Ordinal) || name.EndsWith("or", StringComparison.Ordinal))
            .ToList();
        Assert.Empty(agents);
    }
}
