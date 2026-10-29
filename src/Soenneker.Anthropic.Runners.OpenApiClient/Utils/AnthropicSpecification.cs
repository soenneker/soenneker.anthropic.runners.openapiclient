using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Anthropic.Runners.OpenApiClient.Utils;

/// <summary>Prepares Anthropic's official compressed mock specification for OpenAPI generation.</summary>
public static class AnthropicSpecification
{
    /// <summary>Reads gzip or plain JSON, validates the document, and supplies missing required OpenAPI version metadata.</summary>
    public static async Task Prepare(string source, string destination, CancellationToken cancellationToken = default)
    {
        await using FileStream input = File.OpenRead(source);
        byte[] signature = new byte[2];
        int count = await input.ReadAsync(signature, cancellationToken);
        input.Position = 0;

        JsonNode? document;
        if (count == 2 && signature[0] == 0x1f && signature[1] == 0x8b)
        {
            await using var gzip = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
            document = await JsonNode.ParseAsync(gzip, cancellationToken: cancellationToken);
        }
        else
            document = await JsonNode.ParseAsync(input, cancellationToken: cancellationToken);

        if (document is not JsonObject root || root["openapi"] is null ||
            root["paths"] is not JsonObject paths || paths.Count == 0 || root["info"] is not JsonObject info)
            throw new InvalidDataException("The downloaded document is not an OpenAPI specification with paths and metadata.");

        // The upstream mock spec omits this required OpenAPI field. It is not the anthropic-version request header.
        info["version"] ??= "unversioned";
        await File.WriteAllTextAsync(destination, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
    }
}
