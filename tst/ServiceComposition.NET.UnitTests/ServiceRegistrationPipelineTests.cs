using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceComposition.NET.UnitTests.TestClasses;

namespace ServiceComposition.NET.UnitTests;

public class ServiceRegistrationPipelineTests
{
    [Fact]
    public void Execute_Should_Invoke_Service_Registrations()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipeline();
        pipeline.AddRegistration((x, y) => x.AddScoped<ITestCoreService, TestCoreService>());

        pipeline.Execute(serviceCollection, configuration);
        var provider = serviceCollection.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ITestCoreService>());
    }

    [Fact]
    public void AddRegistration_Throws_WhenExpressionIs_InvalidMethodCall()
    {
        var pipeline = new TestServiceRegistrationPipeline();

        var ex = Assert.Throws<ArgumentException>(() => pipeline.AddRegistration((x, y) => Expression.Empty()));
        var ex2 = Assert.Throws<ArgumentException>(() => pipeline.AddRegistration((x) => Expression.Empty()));

        var  expectedMessage = "Only extension methods declared on IServiceCollection are allowed as service registration expressions.";
        Assert.Contains(expectedMessage, ex.Message);
        Assert.Contains(expectedMessage, ex2.Message);
    }

    [Fact]
    public void RegisterServices_Throws_WhenExpression_Fails()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipeline();
        pipeline.AddRegistration((x, y) => x.ThrowInvalidOperationException());

        Assert.Throws<InvalidOperationException>(() => pipeline.Execute(serviceCollection, configuration));
    }

    [Fact]
    public void Execute_LogsSuccess_WithStartupLogger()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService, TestCoreService>();
        pipeline.AddRegistration(expression);
        var expectedStartedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' completed successfully!";

        using var scope = new StringWriter();
        Console.SetOut(scope);
        pipeline.Execute(serviceCollection, configuration);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_LogsFailure_WithStartupLogger()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.ThrowInvalidOperationException();
        pipeline.AddRegistration(expression);
        var expectedFailureMessage = "'ThrowInvalidOperationException(this IServiceCollection)' failed with an unhandled exception.";

        using var scope = new StringWriter();
        Console.SetOut(scope);
        Assert.Throws<InvalidOperationException>(() => pipeline.Execute(serviceCollection, configuration));

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedFailureMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_LogsServiceRegistration_WithNoParameters()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddService();
        pipeline.AddRegistration(expression);
        var expectedStartedMessage = "'AddService(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddService(this IServiceCollection)' completed successfully!";

        pipeline.Execute(serviceCollection, configuration);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_LogsServiceRegistration_WithGenericParameters()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService, TestCoreService>();
        pipeline.AddRegistration(expression);
        var expectedStartedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' completed successfully!";

        pipeline.Execute(serviceCollection, configuration);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_LogsServiceRegistration_WithNormalParameters()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped(typeof(ITestCoreService), typeof(TestCoreService));
        pipeline.AddRegistration(expression);
        var expectedStartedMessage = "'AddScoped(this IServiceCollection, Type<ServiceComposition.NET.UnitTests.TestClasses.ITestCoreService>, Type<ServiceComposition.NET.UnitTests.TestClasses.TestCoreService>)' was started...";
        var expectedCompletedMessage = "'AddScoped(this IServiceCollection, Type<ServiceComposition.NET.UnitTests.TestClasses.ITestCoreService>, Type<ServiceComposition.NET.UnitTests.TestClasses.TestCoreService>)' completed successfully!";

        pipeline.Execute(serviceCollection, configuration);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_LogsServiceRegistration_WithLambdaExpression()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var pipeline = new TestServiceRegistrationPipelineWithLogger();

        // Lambda expression: AddScoped<ITestCoreService>(services => new TestCoreService())
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService>(services => new TestCoreService());
        pipeline.AddRegistration(expression);

        // Expected messages for logging
        var expectedStartedMessage = "'AddScoped<ITestCoreService>(this IServiceCollection, (IServiceProvider) => new TestCoreService())' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService>(this IServiceCollection, (IServiceProvider) => new TestCoreService())' completed successfully!";

        pipeline.Execute(serviceCollection, configuration);

        // Verify that the logger logged the "started" message with the lambda expression details
        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        // Verify that the logger logged the "completed" message with the lambda expression details
        pipeline.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Execute_Throws_When_ServiceCollection_IsNull()
    {
        var pipeline = new TestServiceRegistrationPipeline();
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() => pipeline.Execute(null!, configuration));
    }
}