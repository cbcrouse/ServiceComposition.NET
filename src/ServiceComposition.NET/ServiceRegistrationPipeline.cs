namespace ServiceComposition.NET;

/// <summary>
/// Represents an ordered pipeline for registering application services and dependencies
/// in a manner that is agnostic to the presentation layer (e.g. Web API, desktop, console, etc.).
/// </summary>
/// <remarks>
/// A <see cref="ServiceRegistrationPipeline"/> defines a sequence of declarative service
/// registration expressions that are executed as a single unit during application startup.
/// The pipeline itself does not own or initialize application configuration or service
/// collection instances; instead, these concerns are supplied by the presentation layer
/// at execution time.
/// <para>
/// This design allows infrastructure layers to participate in service composition without
/// introducing presentation-specific dependencies or assuming a particular hosting model.
/// All service registrations are validated and executed in order when the pipeline is
/// invoked.
/// </para>
/// </remarks>
public abstract class ServiceRegistrationPipeline : ServiceRegistrationExpressionCollection
{
    /// <summary>
    /// Gets a logger used during pipeline execution.
    /// </summary>
    /// <remarks>
    /// This logger is intended for use during service registration execution,
    /// when the application's dependency injection container has not yet been built.
    /// Implementations are responsible for supplying a logger instance that does not
    /// depend on services from the container.
    /// </remarks>
    protected abstract ILogger Logger { get; }

    /// <summary>
    /// Executes the service registration pipeline using the provided service collection
    /// and configuration.
    /// </summary>
    /// <remarks>
    /// This method represents the single execution point of the
    /// <see cref="ServiceRegistrationPipeline"/>. All registered service
    /// registration expressions are executed in the order in which
    /// they were added to the pipeline.
    /// <para>
    /// The pipeline does not create or manage the lifetime of the
    /// <see cref="IServiceCollection"/> or <see cref="IConfiguration"/> instances.
    /// These dependencies are supplied by the presentation layer at execution time.
    /// </para>
    /// <para>
    /// Each registration expression is compiled and invoked during execution, and
    /// detailed trace logging is emitted before and after each registration.
    /// </para>
    /// </remarks>
    /// <param name="serviceCollection">
    /// The <see cref="IServiceCollection"/> instance to which services will be registered.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance used to supply configuration values to
    /// service registrations that require them.
    /// </param>
    public void Execute(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        foreach (Expression<Action<IServiceCollection, IConfiguration>> expression in Expressions)
        {
            var expressionAsString = GetExpressionAsString(expression);

            try
            {
                Logger.LogTrace("'{Expression}' was started...", expressionAsString);
                expression.Compile().Invoke(serviceCollection, configuration);
                Logger.LogTrace("'{Expression}' completed successfully!", expressionAsString);
            }
            catch (Exception exception)
            {
                Logger.LogTrace(exception, "'{Expression}' failed with an unhandled exception.", expressionAsString);
                throw;
            }
        }
    }

    /// <summary>
    /// Converts an expression representing a service registration into a human-readable string.
    /// The string includes the method name, generic arguments (if any), and the parameters passed to the method.
    /// Extension methods are handled by recognizing the 'this' parameter (e.g., <see cref="IServiceCollection"/>).
    /// This method is used to format log messages for service registration activities.
    /// </summary>
    /// <param name="expression">
    /// The <see cref="Expression{Action}"/> object representing the service registration action to be converted to a string.
    /// This expression is expected to be a method call expression.
    /// </param>
    /// <returns>
    /// A formatted string representing the method call, including the method name, any generic arguments,
    /// and the parameters passed to the method. If the method is an extension method, the 'this' parameter is explicitly marked.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown if the expression body is not a <see cref="MethodCallExpression"/>. This method expects the expression body to be a method call.
    /// </exception>
    protected virtual string GetExpressionAsString(Expression<Action<IServiceCollection, IConfiguration>> expression)
    {
        var methodCall = (MethodCallExpression)expression.Body;
        MethodInfo method = methodCall.Method;
        string methodName = methodCall.Method.Name;

        // Handle generic arguments (for cases like AddScoped<IMyService, MyService>)
        string genericArgs = string.Empty;
        if (method.IsGenericMethod)
        {
            var genericArguments = method.GetGenericArguments()
                .Select(arg => arg.Name)
                .ToArray();
            genericArgs = $"<{string.Join(", ", genericArguments)}>";
        }

        // Handle normal parameters (for cases like AddScoped(typeof(IMyService), typeof(MyService)))
        var normalArgs = methodCall.Arguments
            .Select((arg, index) =>
            {
                if (index == 0 && method.IsExtensionMethod())
                {
                    return $"this {arg.Type.Name}";
                }

                if (arg is ConstantExpression constExpr && constExpr.Value != null)
                {
                    return $"{constExpr.Type.Name}<{constExpr.Value}>";
                }

                if (arg is LambdaExpression lambdaExpr)
                {
                    var parameters = lambdaExpr.Parameters.Select(x => x.Type.Name).ToArray();
                    var parameterString = string.Join(",", parameters);
                    return $"({parameterString}) => {lambdaExpr.Body}";
                }

                if (arg is MethodCallExpression methodCallExpr)
                {
                    return methodCallExpr.Method.ReturnType.Name;
                }

                if (arg is TypeBinaryExpression typeBinaryExpr)
                {
                    return typeBinaryExpr.Type.Name;
                }

                return arg.Type.Name;
            })
            .ToArray();

        string parameters = string.Join(", ", normalArgs.Where(x => x != null));

        return $"{methodName}{genericArgs}({parameters})";
    }
}