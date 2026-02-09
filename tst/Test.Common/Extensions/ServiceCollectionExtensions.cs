using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterConfiguredOptions<T>(this IServiceCollection servicesCollection, IConfiguration configuration) where T : class, new()
    {
        string sectionKey = typeof(T).Name;
        IConfigurationSection section = configuration.GetSection(sectionKey.Replace("Options", ""));

        var options = new T();
        section.Bind(options);

        servicesCollection.Configure<T>(section);
    }
}