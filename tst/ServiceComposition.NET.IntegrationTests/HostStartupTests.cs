using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceComposition.NET.IntegrationTests.TestClasses;
using Test.Common.TestClasses;

namespace ServiceComposition.NET.IntegrationTests;

public class HostStartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IServiceProvider _serviceProvider;

    public HostStartupTests(WebApplicationFactory<Program> factory)
    {
        factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            // Required for web app factory to find appsettings.json and other content files in the test project
            builder.UseContentRoot(AppContext.BaseDirectory);
        });

        _serviceProvider = factory.Services;
    }

    [Fact]
    public void Core_ConfiguredOptionsClass_IsRegistered()
    {
        var options = _serviceProvider.GetService<IOptions<TestOptions>>();
        Assert.NotNull(options?.Value);
        Assert.Equal("1", options.Value.Property1);
        Assert.Equal("2", options.Value.Property2);
    }

    [Fact]
    public void Presentation_Service_IsRegistered()
    {
        var service = _serviceProvider.GetService<IService>();
        Assert.NotNull(service);
        Assert.IsType<Service>(service);
    }
}