using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Anthropic.Runners.OpenApiClient.Utils;

internal static class AnthropicGeneratedUrls
{
    public static async Task Fix(string sourceDirectory, CancellationToken cancellationToken)
    {
        foreach (string file in Directory.EnumerateFiles(sourceDirectory, "*RequestBuilder.cs", SearchOption.AllDirectories))
        {
            string source = await File.ReadAllTextAsync(file, cancellationToken);
            // The official mock spec separates beta operations using a literal query in the path.
            // RFC 6570 query continuation must use & when beta=true is already present.
            string updated = source.Replace("?beta=true{?", "?beta=true{&", StringComparison.Ordinal);
            if (source != updated)
                await File.WriteAllTextAsync(file, updated, cancellationToken);
        }
    }
}
