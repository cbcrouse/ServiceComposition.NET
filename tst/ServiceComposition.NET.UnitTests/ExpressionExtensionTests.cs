using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposition.NET.UnitTests.TestClasses;
using Test.Common.TestClasses;

namespace ServiceComposition.NET.UnitTests;

public class ExpressionExtensionTests
{
    // ------------------------------------------------------------
    // IConfiguration-aware overload
    // ------------------------------------------------------------

    [Fact]
    public void ValidateServiceRegistration_WithConfiguration_Throws_WhenExpressionIsNotMethodCall()
    {
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = null!;

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Registration expression must be a call to a method on IServiceCollection.", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithConfiguration_Throws_WhenMethodIsNotIServiceCollectionExtension()
    {
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = (_, config) => config.ConfigurationExtension();

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Only extension methods declared on IServiceCollection are allowed", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithConfiguration_Passes_ForValidServiceCollectionExtensions()
    {
        Expression<Action<IServiceCollection, IConfiguration>> nonGeneric = (services, _) => services.AddTransient(typeof(IService), typeof(Service));
        Expression<Action<IServiceCollection, IConfiguration>> generic = (services, _) => services.AddTransient<IService, Service>();
        Expression<Action<IServiceCollection, IConfiguration>> customExtension = (services, _) => services.ServiceCollectionExtension();

        Assert.Null(Record.Exception(() => nonGeneric.ValidateServiceRegistration()));
        Assert.Null(Record.Exception(() => generic.ValidateServiceRegistration()));
        Assert.Null(Record.Exception(() => customExtension.ValidateServiceRegistration()));
    }

    // ------------------------------------------------------------
    // Configuration-free overload
    // ------------------------------------------------------------

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenExpressionIsNotMethodCall()
    {
        Expression<Action<IServiceCollection>> registrationExpression = null!;

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Registration expression must be a call to a method on IServiceCollection.", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenMethodIsNotIServiceCollectionExtension()
    {
        Expression<Action<IServiceCollection>> registrationExpression = services => InvalidStaticMethod();

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Only extension methods declared on IServiceCollection are allowed", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Passes_ForValidServiceCollectionExtensions()
    {
        Expression<Action<IServiceCollection>> generic = services => services.AddTransient<IService, Service>();
        Expression<Action<IServiceCollection>> nonGeneric = services => services.AddTransient(typeof(IService), typeof(Service));
        Expression<Action<IServiceCollection>> customExtension = services => services.ServiceCollectionExtension();

        Assert.Null(Record.Exception(() => generic.ValidateServiceRegistration()));
        Assert.Null(Record.Exception(() => nonGeneric.ValidateServiceRegistration()));
        Assert.Null(Record.Exception(() => customExtension.ValidateServiceRegistration()));
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private static void InvalidStaticMethod()
    {
        // Intentionally empty
    }
}
