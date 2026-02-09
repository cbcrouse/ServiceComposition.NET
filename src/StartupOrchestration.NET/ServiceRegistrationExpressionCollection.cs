namespace StartupOrchestration.NET;

/// <summary>
/// Provides shared infrastructure for managing a validated, ordered collection
/// of service registration expressions.
/// </summary>
/// <remarks>
/// This type centralizes the mechanics for collecting, validating, and normalizing
/// service registration expressions without performing any execution. It exists to
/// avoid duplication of expression management behavior across higher-level
/// composition types, such as service registration pipelines and presentation-layer
/// composition roots.
/// <para>
/// All expressions added to this collection are validated at the point of mutation
/// and normalized into a configuration-aware form
/// (<c>Expression&lt;Action&lt;IServiceCollection, IConfiguration&gt;&gt;</c>). Expressions
/// that do not require configuration are lifted into this canonical shape without
/// being compiled or invoked.
/// </para>
/// <para>
/// This class intentionally contains no execution logic and does not assume a
/// particular hosting model, startup lifecycle, or ownership of configuration or
/// service collection instances. Execution semantics are the responsibility of
/// higher-level abstractions.
/// </para>
/// <para>
/// Expressions managed by this collection are expected to represent service
/// registration operations performed against <see cref="IServiceCollection"/>,
/// such as calls to <c>AddTransient</c>, <c>AddScoped</c>, <c>AddSingleton</c>, or
/// valid extension methods defined for <see cref="IServiceCollection"/>.
/// </para>
/// <para>
/// This type is infrastructure support for higher-level composition abstractions
/// and is not intended to be used or extended directly by application code.
/// </para>
/// </remarks>
public abstract class ServiceRegistrationExpressionCollection
{
    private readonly List<Expression<Action<IServiceCollection, IConfiguration>>> _expressions = new();

    /// <summary>
    /// Gets the ordered collection of validated service registration expressions.
    /// </summary>
    /// <remarks>
    /// The returned collection reflects the order in which expressions were added.
    /// This ordering is preserved and relied upon by consumers that execute the
    /// expressions as part of a larger service composition process.
    /// </remarks>
    protected IReadOnlyList<Expression<Action<IServiceCollection, IConfiguration>>> Expressions => _expressions;

    /// <summary>
    /// Adds a service registration expression that requires access to configuration.
    /// </summary>
    /// <remarks>
    /// The provided expression must represent a valid service registration call
    /// against <see cref="IServiceCollection"/>, such as <c>AddTransient</c>,
    /// <c>AddScoped</c>, <c>AddSingleton</c>, or a custom extension method defined
    /// for <see cref="IServiceCollection"/>.
    /// <para>
    /// Expressions are validated at the point of addition and are not compiled or
    /// executed until a higher-level component invokes the service registration
    /// pipeline.
    /// </para>
    /// </remarks>
    /// <param name="expression">
    /// The service registration expression to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid service registration call.
    /// </exception>
    public void AddRegistration(Expression<Action<IServiceCollection, IConfiguration>> expression)
    {
        expression.ValidateServiceRegistration();
        _expressions.Add(expression);
    }

    /// <summary>
    /// Adds a service registration expression that does not require access to configuration.
    /// </summary>
    /// <remarks>
    /// The provided expression must represent a valid service registration call
    /// against <see cref="IServiceCollection"/>, such as <c>AddTransient</c>,
    /// <c>AddScoped</c>, <c>AddSingleton</c>, or a custom extension method defined
    /// for <see cref="IServiceCollection"/>.
    /// <para>
    /// The expression is lifted into a configuration-aware form without being
    /// compiled or executed. Execution occurs only when the service registration
    /// pipeline is invoked.
    /// </para>
    /// </remarks>
    /// <param name="expression">
    /// The service registration expression to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid service registration call.
    /// </exception>
    public void AddRegistration(Expression<Action<IServiceCollection>> expression)
    {
        expression.ValidateServiceRegistration();

        var servicesParameter = expression.Parameters[0];
        var configurationParameter =
            Expression.Parameter(typeof(IConfiguration), "configuration");

        var liftedExpression =
            Expression.Lambda<Action<IServiceCollection, IConfiguration>>(
                expression.Body,
                servicesParameter,
                configurationParameter);

        liftedExpression.ValidateServiceRegistration();
        _expressions.Add(liftedExpression);
    }

    /// <summary>
    /// Adds multiple service registration expressions that require access to configuration.
    /// </summary>
    /// <param name="expressions">
    /// The collection of service registration expressions to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expressions"/> is <c>null</c>.
    /// </exception>
    public void AddRegistrations(IEnumerable<Expression<Action<IServiceCollection, IConfiguration>>> expressions)
    {
        foreach (var expression in expressions)
        {
            AddRegistration(expression);
        }
    }

    /// <summary>
    /// Adds multiple service registration expressions that do not require access to configuration.
    /// </summary>
    /// <remarks>
    /// Each expression is lifted into a configuration-aware form without being
    /// compiled or executed.
    /// </remarks>
    /// <param name="expressions">
    /// The collection of service registration expressions to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expressions"/> is <c>null</c>.
    /// </exception>
    public void AddRegistrations(IEnumerable<Expression<Action<IServiceCollection>>> expressions)
    {
        foreach (var expression in expressions)
        {
            AddRegistration(expression);
        }
    }
}
