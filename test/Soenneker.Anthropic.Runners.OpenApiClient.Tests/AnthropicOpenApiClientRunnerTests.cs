using Soenneker.Tests.HostedUnit;

namespace Soenneker.Anthropic.Runners.OpenApiClient.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class AnthropicOpenApiClientRunnerTests : HostedUnitTest
{
    public AnthropicOpenApiClientRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
