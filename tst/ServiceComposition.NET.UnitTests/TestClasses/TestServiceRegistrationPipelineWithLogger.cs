using Microsoft.Extensions.Logging;

namespace ServiceComposition.NET.UnitTests.TestClasses;

internal sealed class TestServiceRegistrationPipelineWithLogger : ServiceRegistrationPipeline
{
    private readonly Mock<ILogger> _mockLogger = new();

    protected override ILogger Logger => _mockLogger.Object;

    internal Mock<ILogger> GetLogger()
    {
        return _mockLogger;
    }
}