using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ServiceComposition.NET.IntegrationTests.TestClasses;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Test:Property1"] = "1",
    ["Test:Property2"] = "2"
});

var serviceComposition = new TestServiceCompositionRoot();
serviceComposition.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => Results.Ok());

if (!app.Environment.IsEnvironment("Test"))
{
    await app.RunAsync();
}