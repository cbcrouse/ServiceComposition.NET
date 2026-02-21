using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceComposition.NET.UnitTests;

public class ServiceRegistrationExpressionCollectionTests
{
    private sealed class TestCollection : ServiceRegistrationExpressionCollection
    {
        public IReadOnlyList<Expression<Action<IServiceCollection, IConfiguration>>> GetExpressions() => Expressions;
    }

    private interface ITestService;
    private class TestService : ITestService;

    #region AddRegistration (Configuration Aware)

    [Fact]
    public void AddRegistration_WithConfiguration_AddsExpression()
    {
        var collection = new TestCollection();

        Expression<Action<IServiceCollection, IConfiguration>> expr =
            (services, config) => services.AddScoped<ITestService, TestService>();

        collection.AddRegistration(expr);

        Assert.Single(collection.GetExpressions());
    }

    [Fact]
    public void AddRegistration_WithConfiguration_Null_Throws()
    {
        var collection = new TestCollection();

        Assert.Throws<ArgumentNullException>(() =>
            collection.AddRegistration((Expression<Action<IServiceCollection, IConfiguration>>)null!));
    }

    #endregion

    #region AddRegistration (Without Configuration)

    [Fact]
    public void AddRegistration_WithoutConfiguration_AddsLiftedExpression()
    {
        var collection = new TestCollection();

        Expression<Action<IServiceCollection>> expr =
            services => services.AddSingleton<ITestService, TestService>();

        collection.AddRegistration(expr);

        var stored = collection.GetExpressions().Single();

        Assert.NotNull(stored);
        Assert.Equal(2, stored.Parameters.Count);
        Assert.Equal(typeof(IServiceCollection), stored.Parameters[0].Type);
        Assert.Equal(typeof(IConfiguration), stored.Parameters[1].Type);
    }

    [Fact]
    public void AddRegistration_WithoutConfiguration_Null_Throws()
    {
        var collection = new TestCollection();

        Assert.Throws<ArgumentNullException>(() =>
            collection.AddRegistration((Expression<Action<IServiceCollection>>)null!));
    }

    #endregion

    #region AddRegistrations (Configuration Aware)

    [Fact]
    public void AddRegistrations_WithConfiguration_AddsAllInOrder()
    {
        var collection = new TestCollection();

        var expr1 = (Expression<Action<IServiceCollection, IConfiguration>>)
            ((s, c) => s.AddScoped<ITestService, TestService>());

        var expr2 = (Expression<Action<IServiceCollection, IConfiguration>>)
            ((s, c) => s.AddSingleton<ITestService, TestService>());

        collection.AddRegistrations([expr1, expr2]);

        var stored = collection.GetExpressions();

        Assert.Equal(2, stored.Count);
        Assert.Same(expr1, stored[0]);
        Assert.Same(expr2, stored[1]);
    }

    [Fact]
    public void AddRegistrations_WithConfiguration_NullCollection_Throws()
    {
        var collection = new TestCollection();

        Assert.Throws<ArgumentNullException>(() =>
            collection.AddRegistrations((IEnumerable<Expression<Action<IServiceCollection, IConfiguration>>>)null!));
    }

    #endregion

    #region AddRegistrations (Without Configuration)

    [Fact]
    public void AddRegistrations_WithoutConfiguration_AddsAllLifted()
    {
        var collection = new TestCollection();

        var expr1 = (Expression<Action<IServiceCollection>>)
            (s => s.AddScoped<ITestService, TestService>());

        var expr2 = (Expression<Action<IServiceCollection>>)
            (s => s.AddSingleton<ITestService, TestService>());

        collection.AddRegistrations([expr1, expr2]);

        var stored = collection.GetExpressions();

        Assert.Equal(2, stored.Count);
        Assert.All(stored, e => Assert.Equal(2, e.Parameters.Count));
    }

    [Fact]
    public void AddRegistrations_WithoutConfiguration_NullCollection_Throws()
    {
        var collection = new TestCollection();

        Assert.Throws<ArgumentNullException>(() =>
            collection.AddRegistrations((IEnumerable<Expression<Action<IServiceCollection>>>)null!));
    }

    #endregion

    #region Validation Behavior

    [Fact]
    public void AddRegistration_InvalidTargetMethod_Throws()
    {
        var collection = new TestCollection();

        Expression<Action<IServiceCollection>> invalid =
            services => services.ToString(); // Not IServiceCollection extension

        Assert.Throws<ArgumentException>(() =>
            collection.AddRegistration(invalid));
    }

    [Fact]
    public void AddRegistrations_StopsOnFirstInvalidExpression()
    {
        var collection = new TestCollection();

        var valid = (Expression<Action<IServiceCollection>>)
            (s => s.AddScoped<ITestService, TestService>());

        var invalid = (Expression<Action<IServiceCollection>>)
            (s => s.ToString());

        Assert.Throws<ArgumentException>(() =>
            collection.AddRegistrations([valid, invalid]));

        // Only the first valid one should have been added
        Assert.Single(collection.GetExpressions());
    }

    #endregion

    #region Ordering Guarantee

    [Fact]
    public void Expressions_PreserveInsertionOrder()
    {
        var collection = new TestCollection();

        var expr1 = (Expression<Action<IServiceCollection>>)
            (s => s.AddScoped<ITestService, TestService>());

        var expr2 = (Expression<Action<IServiceCollection>>)
            (s => s.AddSingleton<ITestService, TestService>());

        collection.AddRegistration(expr1);
        collection.AddRegistration(expr2);

        var stored = collection.GetExpressions();

        Assert.Equal(2, stored.Count);
        Assert.Equal("AddScoped", ((MethodCallExpression)stored[0].Body).Method.Name);
        Assert.Equal("AddSingleton", ((MethodCallExpression)stored[1].Body).Method.Name);
    }

    #endregion
}