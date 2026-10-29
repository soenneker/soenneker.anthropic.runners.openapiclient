using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Anthropic.Runners.OpenApiClient.Utils.Abstract;

/// <summary>Regenerates the Anthropic API client from Anthropic's published gzip-compressed OpenAPI specification.</summary>
public interface IFileOperationsUtil
{
    /// <summary>
    /// Downloads the specification, regenerates the client, and verifies a Release build.
    /// Set Anthropic:LocalRepositoryPath to regenerate a local client repository without pushing by default.
    /// Anthropic:PushChanges explicitly controls publication; Anthropic:ClientGenerationUrl overrides the specification source.
    /// Throws when generation or compilation fails.
    /// </summary>
    ValueTask Process(CancellationToken cancellationToken = default);
}
