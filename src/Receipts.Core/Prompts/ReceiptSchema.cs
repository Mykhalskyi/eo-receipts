using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// Schema of a receipt. Kept within the subset understood by structured output
/// of all supported providers: every property required, no additional properties,
/// optional values expressed as empty strings or <c>anyOf</c> with null.
/// </summary>
public sealed class ReceiptSchema : IJsonSchema
{
    private const string Text =
        """
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["merchant", "date", "currency", "lines", "taxes", "subtotal", "total", "payment"],
          "properties": {
            "merchant": {
              "type": "object",
              "additionalProperties": false,
              "required": ["name", "address", "taxId"],
              "properties": {
                "name": { "type": "string" },
                "address": { "type": "string" },
                "taxId": { "type": "string", "description": "VAT, TIN or another tax number of the merchant" }
              }
            },
            "date": { "type": "string", "description": "ISO 8601 date or date-time, empty when absent" },
            "currency": { "type": "string", "description": "ISO 4217 code, XXX when unknown" },
            "lines": {
              "type": "array",
              "items": {
                "type": "object",
                "additionalProperties": false,
                "required": ["name", "quantity", "unitPrice", "total"],
                "properties": {
                  "name": { "type": "string" },
                  "quantity": { "type": "number" },
                  "unitPrice": { "type": "number" },
                  "total": { "type": "number" }
                }
              }
            },
            "taxes": {
              "type": "array",
              "items": {
                "type": "object",
                "additionalProperties": false,
                "required": ["name", "rate", "amount"],
                "properties": {
                  "name": { "type": "string" },
                  "rate": { "type": "number", "description": "percent, 20 for 20%" },
                  "amount": { "type": "number" }
                }
              }
            },
            "subtotal": { "anyOf": [{ "type": "number" }, { "type": "null" }] },
            "total": { "type": "number" },
            "payment": {
              "type": "object",
              "additionalProperties": false,
              "required": ["method", "cardLast4"],
              "properties": {
                "method": { "type": "string" },
                "cardLast4": { "type": "string" }
              }
            }
          }
        }
        """;

    private readonly IJsonSchema origin;

    public ReceiptSchema()
    {
        this.origin = new JsonSchema(Text);
    }

    public JsonObject Json()
    {
        return this.origin.Json();
    }
}
