using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceComposition.NET.UnitTests.TestClasses
{
    internal static class TestExtensions
    {
        internal static void ServiceCollectionExtension(this IServiceCollection services)
        {
            // Do nothing
        }
        internal static void ConfigurationExtension(this IConfiguration config)
        {
            // Do nothing
        }

        internal static void ThrowInvalidOperationException(this IServiceCollection services)
        {
            throw new InvalidOperationException();
        }

        internal static IServiceCollection AddService(this IServiceCollection services)
        {
            return services;
        }
    }
}