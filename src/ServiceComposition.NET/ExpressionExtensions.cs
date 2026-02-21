namespace ServiceComposition.NET;

/// <summary>
/// Provides extension methods for validating service registration expressions
/// used within service composition pipelines.
/// </summary>
/// <remarks>
/// These validation methods enforce the structural constraints required by
/// <see cref="ServiceRegistrationPipeline"/> and
/// <see cref="ServiceRegistrationExpressionCollection"/>.
/// <para>
/// A valid service registration expression must:
/// <list type="bullet">
/// <item>
/// Represent a single method call expression.
/// </item>
/// <item>
/// Invoke an extension method.
/// </item>
/// <item>
/// Target <see cref="IServiceCollection"/> as the extension method's first parameter.
/// </item>
/// </list>
/// </para>
/// Validation occurs at the time an expression is added to a pipeline,
/// ensuring invalid registrations are rejected before execution.
/// </remarks>
public static class ExpressionExtensions
{
    /// <summary>
    /// Validates a service registration expression that requires access to configuration.
    /// </summary>
    /// <param name="registrationExpression">
    /// A lambda expression representing a service registration operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registrationExpression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression is not a valid service registration call.
    /// </exception>
    /// <remarks>
    /// The expression must represent a single extension method call on
    /// <see cref="IServiceCollection"/>. For example:
    /// <para>
    /// <c>(services, configuration) =&gt; services.AddScoped&lt;IMyService, MyService&gt;()</c>
    /// </para>
    /// </remarks>
    public static void ValidateServiceRegistration(this Expression<Action<IServiceCollection, IConfiguration>>? registrationExpression) =>
        ValidateCore(registrationExpression);

    /// <summary>
    /// Validates a service registration expression that does not require configuration.
    /// </summary>
    /// <param name="registrationExpression">
    /// A lambda expression representing a service registration operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registrationExpression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression is not a valid service registration call.
    /// </exception>
    /// <remarks>
    /// The expression must represent a single extension method call on
    /// <see cref="IServiceCollection"/>. For example:
    /// <para>
    /// <c>services =&gt; services.AddSingleton&lt;IMyService, MyService&gt;()</c>
    /// </para>
    /// </remarks>
    public static void ValidateServiceRegistration(this Expression<Action<IServiceCollection>>? registrationExpression) =>
        ValidateCore(registrationExpression);

    /// <summary>
    /// Performs core validation logic for service registration expressions.
    /// </summary>
    /// <param name="registrationExpression">
    /// The lambda expression to validate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registrationExpression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression is not a single method call targeting a valid
    /// <see cref="IServiceCollection"/> extension method.
    /// </exception>
    private static void ValidateCore(LambdaExpression? registrationExpression)
    {
        if (registrationExpression is null)
            throw new ArgumentNullException(nameof(registrationExpression));

        if (registrationExpression.Body is not MethodCallExpression methodCallExpression)
            throw new ArgumentException(
                "Registration expression must be a single method call on IServiceCollection.",
                nameof(registrationExpression));

        methodCallExpression.EnsureValidServiceCollectionExtension();
    }

    /// <summary>
    /// Ensures the method call represents a valid <see cref="IServiceCollection"/> extension.
    /// </summary>
    /// <param name="methodCallExpression">
    /// The method call expression to validate.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the method is not an extension method declared for
    /// <see cref="IServiceCollection"/>.
    /// </exception>
    private static void EnsureValidServiceCollectionExtension(
        this MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        var isAssignableFromIServiceCollection =
            method.GetParameters().Length > 0 &&
            typeof(IServiceCollection).IsAssignableFrom(method.GetParameters()[0].ParameterType);

        var isValid = method.IsExtensionMethod() && isAssignableFromIServiceCollection;

        if (!isValid)
            throw new ArgumentException("Only extension methods declared on IServiceCollection are allowed as service registration expressions.");
    }

    /// <summary>
    /// Determines whether a method is defined as an extension method.
    /// </summary>
    /// <param name="methodInfo">
    /// The method to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the method is marked with <see cref="System.Runtime.CompilerServices.ExtensionAttribute"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool IsExtensionMethod(this MethodInfo methodInfo) =>
        methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
}