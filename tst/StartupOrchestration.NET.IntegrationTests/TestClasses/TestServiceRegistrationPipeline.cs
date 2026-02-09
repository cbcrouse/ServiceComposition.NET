using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Test.Common.Extensions;

namespace StartupOrchestration.NET.IntegrationTests.TestClasses;

internal class TestServiceRegistrationPipeline : ServiceRegistrationPipeline
{
    /// <inheritdoc />
    protected override ILogger StartupLogger => NullLogger.Instance;

    public TestServiceRegistrationPipeline()
    {
        AddRegistration((services, config) => services.RegisterConfiguredOptions<TestOptions>(config));
    }
}
