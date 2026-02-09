using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StartupOrchestration.NET.IntegrationTests.TestClasses;
using StartupOrchestration.NET.UnitTests.TestClasses;

namespace StartupOrchestration.NET.UnitTests;

public class ServiceCompositionRootTests
{
    [Fact]
    public void ConfigureServices_RegistersCoreAndPresentationServices()
    {
        // Arrange
        var root = new TestServiceCompositionRoot();
        root.AddRegistration(
            services => services.AddScoped<ITestPresentationService, TestPresentationService>());

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        // Act
        root.ConfigureServices(services, configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ITestCoreService>());
        Assert.NotNull(provider.GetService<ITestPresentationService>());
    }

    [Fact]
    public void PipelineRegistrations_AreExecutedBefore_PresentationRegistrations()
    {
        // Arrange
        var root = new TestServiceCompositionRoot();

        root.AddRegistration(services =>
            services.AddSingleton<ITestPresentationService, TestPresentationService>());

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        root.ConfigureServices(services, configuration);

        // Assert
        var descriptors = services.ToList();

        var coreIndex = descriptors.FindIndex(
            d => d.ServiceType == typeof(ITestCoreService));

        var presentationIndex = descriptors.FindIndex(
            d => d.ServiceType == typeof(ITestPresentationService));

        Assert.True(coreIndex >= 0);
        Assert.True(presentationIndex >= 0);
        Assert.True(coreIndex < presentationIndex);
    }

    [Fact]
    public void ConfigureServices_UsesConfigurationSuppliedByCaller()
    {
        // Arrange
        var root = new TestServiceCompositionRoot();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Test:Property1"] = "A",
                ["Test:Property2"] = "B"
            })
            .Build();

        // Act
        root.ConfigureServices(services, configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TestOptions>>();

        Assert.Equal("A", options.Value.Property1);
        Assert.Equal("B", options.Value.Property2);
    }
}
