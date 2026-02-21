namespace ServiceComposition.NET;

/// <summary>
/// Provides shared infrastructure for managing a validated, ordered collection
/// of service registration expressions.
/// </summary>
/// <remarks>
/// This type centralizes the mechanics for collecting, validating, and normalizing
/// service registration expressions without executing them.
/// <para>
/// Expressions are validated at the point of addition to ensure they represent
/// legitimate extension method calls targeting <see cref="IServiceCollection"/>.
/// Invalid expressions are rejected immediately and never stored.
/// </para>
/// <para>
/// All stored expressions are normalized into a configuration-aware canonical form:
/// <c>Expression&lt;Action&lt;IServiceCollection, IConfiguration&gt;&gt;</c>.
/// Expressions that do not require configuration are lifted into this shape
/// without being compiled or executed.
/// </para>
/// <para>
/// This class contains no execution logic and does not assume ownership of
/// <see cref="IServiceCollection"/> or <see cref="IConfiguration"/> instances.
/// Execution semantics are the responsibility of higher-level abstractions,
/// such as <see cref="ServiceRegistrationPipeline"/>.
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
    /// The returned collection preserves the order in which expressions were added.
    /// Ordering is significant and relied upon by consumers that execute the
    /// collection as part of a service composition pipeline.
    /// </remarks>
    protected IReadOnlyList<Expression<Action<IServiceCollection, IConfiguration>>> Expressions => _expressions;

    /// <summary>
    /// Adds a service registration expression that requires access to configuration.
    /// </summary>
    /// <param name="expression">
    /// A lambda expression representing a service registration operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid
    /// <see cref="IServiceCollection"/> extension method call.
    /// </exception>
    /// <remarks>
    /// The expression must represent a single extension method call targeting
    /// <see cref="IServiceCollection"/>. Validation occurs immediately and the
    /// expression is not compiled or executed at this stage.
    /// </remarks>
    public void AddRegistration(Expression<Action<IServiceCollection, IConfiguration>> expression)
    {
        expression.ValidateServiceRegistration();
        _expressions.Add(expression);
    }

    /// <summary>
    /// Adds a service registration expression that does not require configuration.
    /// </summary>
    /// <param name="expression">
    /// A lambda expression representing a service registration operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid
    /// <see cref="IServiceCollection"/> extension method call.
    /// </exception>
    /// <remarks>
    /// The expression is validated and then lifted into a configuration-aware
    /// canonical form without being compiled or executed. The lifted expression
    /// preserves the original method call and defers execution until a
    /// <see cref="ServiceRegistrationPipeline"/> invokes it.
    /// </remarks>
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
    /// Adds multiple service registration expressions that require configuration.
    /// </summary>
    /// <param name="expressions">
    /// A sequence of service registration expressions to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expressions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any expression in the sequence is invalid.
    /// </exception>
    /// <remarks>
    /// Expressions are validated individually in the order provided.
    /// If validation fails for any expression, no further expressions are added.
    /// </remarks>
    public void AddRegistrations(IEnumerable<Expression<Action<IServiceCollection, IConfiguration>>>? expressions)
    {
        if (expressions is null)
            throw new ArgumentNullException(nameof(expressions));

        foreach (var expression in expressions)
        {
            AddRegistration(expression);
        }
    }

    /// <summary>
    /// Adds multiple service registration expressions that do not require configuration.
    /// </summary>
    /// <param name="expressions">
    /// A sequence of service registration expressions to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expressions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any expression in the sequence is invalid.
    /// </exception>
    /// <remarks>
    /// Each expression is validated and lifted into a configuration-aware form.
    /// Execution remains deferred until a pipeline invokes the collection.
    /// </remarks>
    public void AddRegistrations(IEnumerable<Expression<Action<IServiceCollection>>>? expressions)
    {
        if (expressions is null)
            throw new ArgumentNullException(nameof(expressions));

        foreach (var expression in expressions)
        {
            AddRegistration(expression);
        }
    }
}