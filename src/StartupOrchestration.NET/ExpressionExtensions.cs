namespace StartupOrchestration.NET;

/// <summary>
/// Provides extension methods for working with <see cref="Expression"/> objects.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Validates a service registration expression that requires access to configuration.
    /// </summary>
    /// <param name="registrationExpression">
    /// The service registration expression to validate.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid service registration call.
    /// </exception>
    public static void ValidateServiceRegistration(this Expression<Action<IServiceCollection, IConfiguration>> registrationExpression)
    {
        if (registrationExpression?.Body is not MethodCallExpression methodCallExpression)
        {
            throw new ArgumentException(
                "Registration expression must be a call to a method on IServiceCollection.",
                nameof(registrationExpression));
        }

        if (!methodCallExpression.IsValidServiceCollectionExtension())
        {
            throw new ArgumentException(
                "Only extension methods declared on IServiceCollection are allowed as service registration expressions.",
                nameof(registrationExpression));
        }
    }

    /// <summary>
    /// Validates a service registration expression that does not require access to configuration.
    /// </summary>
    /// <param name="registrationExpression">
    /// The service registration expression to validate.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a valid service registration call.
    /// </exception>
    public static void ValidateServiceRegistration(this Expression<Action<IServiceCollection>> registrationExpression)
    {
        if (registrationExpression?.Body is not MethodCallExpression methodCallExpression)
        {
            throw new ArgumentException(
                "Registration expression must be a call to a method on IServiceCollection.",
                nameof(registrationExpression));
        }

        if (!methodCallExpression.IsValidServiceCollectionExtension())
        {
            throw new ArgumentException(
                "Only extension methods declared on IServiceCollection are allowed as service registration expressions.",
                nameof(registrationExpression));
        }
    }

    private static bool IsValidServiceCollectionExtension(this MethodCallExpression methodCallExpression)
    {
        return methodCallExpression.Method.IsExtensionMethod() &&
               typeof(IServiceCollection).IsAssignableFrom(methodCallExpression.Method.GetParameters()[0].ParameterType);
    }

    private static bool IsExtensionMethod(this MethodInfo methodInfo)
    {
        return methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
    }
}