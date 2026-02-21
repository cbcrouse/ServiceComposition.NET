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
    public void ValidateServiceRegistration_WithConfiguration_Throws_WhenExpressionIsNull()
    {
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains($"{nameof(registrationExpression)}", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_Throws_WhenExtensionDoesNotTargetIServiceCollection()
    {
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = (_, config) => config.ConfigurationExtension();

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Only extension methods declared on IServiceCollection are allowed", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_Throws_WhenMethodIsInstanceMethod()
    {
        Expression<Action<IServiceCollection, IConfiguration>> expr = (services, config) => services.ToString();

        var ex = Assert.Throws<ArgumentException>(() => expr.ValidateServiceRegistration());

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

    [Fact]
    public void ValidateServiceRegistration_Throws_WhenExpressionIsInvocation()
    {
        Expression<Action<IServiceCollection, IConfiguration>> inner =
            (s, c) => s.AddTransient<IService, Service>();

        var serviceParam = Expression.Parameter(typeof(IServiceCollection), "s");
        var configParam = Expression.Parameter(typeof(IConfiguration), "c");

        var invocation = Expression.Invoke(inner, serviceParam, configParam);

        var lambda = Expression.Lambda<Action<IServiceCollection, IConfiguration>>(
            invocation,
            serviceParam,
            configParam);

        var ex = Assert.Throws<ArgumentException>(() =>
            lambda.ValidateServiceRegistration());

        Assert.Contains("single method call", ex.Message);
    }

    // ------------------------------------------------------------
    // Configuration-free overload
    // ------------------------------------------------------------

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenExpressionIsNull()
    {
        Expression<Action<IServiceCollection>> registrationExpression = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains($"{nameof(registrationExpression)}", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenExtensionDoesNotTargetIServiceCollection()
    {
        Expression<Action<IServiceCollection>> registrationExpression = services => InvalidStaticMethod();

        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());

        Assert.Contains("Only extension methods declared on IServiceCollection are allowed", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenMethodIsInstanceMethod()
    {
        Expression<Action<IServiceCollection>> expr = services => services.ToString();

        var ex = Assert.Throws<ArgumentException>(() => expr.ValidateServiceRegistration());

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

    [Fact]
    public void ValidateServiceRegistration_WithoutConfiguration_Throws_WhenExpressionIsInvocation()
    {
        Expression<Action<IServiceCollection>> inner =
            s => s.AddTransient<IService, Service>();

        var parameter = Expression.Parameter(typeof(IServiceCollection), "s");

        var invocation = Expression.Invoke(inner, parameter);

        var lambda = Expression.Lambda<Action<IServiceCollection>>(
            invocation,
            parameter);

        var ex = Assert.Throws<ArgumentException>(() =>
            lambda.ValidateServiceRegistration());

        Assert.Contains("single method call", ex.Message);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private static void InvalidStaticMethod()
    {
        // Intentionally empty
    }
}
