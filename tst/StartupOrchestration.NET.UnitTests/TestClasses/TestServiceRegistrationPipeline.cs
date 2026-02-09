using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartupOrchestration.NET.IntegrationTests.TestClasses;
using Test.Common.Extensions;

namespace StartupOrchestration.NET.UnitTests.TestClasses;

internal sealed class TestServiceRegistrationPipeline : ServiceRegistrationPipeline
{
    protected override ILogger StartupLogger => NullLogger.Instance;

    public TestServiceRegistrationPipeline()
    {
        AddRegistration((services, config) => services.RegisterConfiguredOptions<TestOptions>(config));
        AddRegistration((services, _) => services.AddSingleton<ITestCoreService, TestCoreService>());
    }
}