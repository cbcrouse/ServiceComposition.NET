using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestClasses;

namespace ServiceComposition.NET.IntegrationTests.TestClasses;

internal sealed class TestServiceCompositionRoot : ServiceCompositionRoot<TestServiceRegistrationPipeline>
{
    public TestServiceCompositionRoot()
    {
        AddRegistration(services => services.AddTransient<IService, Service>());
    }
}