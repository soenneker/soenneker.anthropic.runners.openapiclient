using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Soenneker.Anthropic.Runners.OpenApiClient.Utils;

namespace Soenneker.Anthropic.Runners.OpenApiClient.Tests;

public sealed class AnthropicSpecificationTests
{
    [Test]
    public async Task DecompressesOfficialShapeAndPreservesOperations()
    {
        string folder = Path.Combine(Path.GetTempPath(), "anthropic-spec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "spec.json.gz");
            string destination = Path.Combine(folder, "openapi.json");
            const string json = """{"openapi":"3.1.0","info":{"title":"Anthropic API"},"paths":{"/v1/messages":{"post":{"operationId":"createMessage"}}}}""";
            await using (FileStream stream = File.Create(source))
            await using (var gzip = new GZipStream(stream, CompressionMode.Compress))
                await gzip.WriteAsync(Encoding.UTF8.GetBytes(json));

            await AnthropicSpecification.Prepare(source, destination);
            JsonNode document = JsonNode.Parse(await File.ReadAllTextAsync(destination))!;
            if (document["info"]!["version"]!.ToString() != "unversioned" ||
                document["paths"]!["/v1/messages"]!["post"]!["operationId"]!.ToString() != "createMessage")
                throw new Exception("Decompression changed the specification.");
        }
        finally { Directory.Delete(folder, true); }
    }

    [Test]
    public async Task AcceptsPlainJsonWithoutReplacingExistingVersion()
    {
        string folder = Path.Combine(Path.GetTempPath(), "anthropic-spec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "input.json");
            string destination = Path.Combine(folder, "output.json");
            await File.WriteAllTextAsync(source, """{"openapi":"3.1.0","info":{"title":"Anthropic API","version":"existing"},"paths":{"/v1/models":{"get":{}}}}""");
            await AnthropicSpecification.Prepare(source, destination);
            JsonNode document = JsonNode.Parse(await File.ReadAllTextAsync(destination))!;
            if (document["info"]!["version"]!.ToString() != "existing")
                throw new Exception("An upstream version was replaced.");
        }
        finally { Directory.Delete(folder, true); }
    }

    [Test]
    public async Task RejectsNonSpecificationWithoutReplacingOutput()
    {
        string folder = Path.Combine(Path.GetTempPath(), "anthropic-spec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "input.json");
            string destination = Path.Combine(folder, "output.json");
            await File.WriteAllTextAsync(source, """{"error":"not a spec"}""");
            await File.WriteAllTextAsync(destination, "previous");
            try { await AnthropicSpecification.Prepare(source, destination); }
            catch (InvalidDataException)
            {
                if (await File.ReadAllTextAsync(destination) != "previous")
                    throw new Exception("Invalid input replaced the prior specification.");
                return;
            }
            throw new Exception("A non-specification was accepted.");
        }
        finally { Directory.Delete(folder, true); }
    }
}
