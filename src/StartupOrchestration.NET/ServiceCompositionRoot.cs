namespace StartupOrchestration.NET;

/// <summary>
/// Serves as the composition root for the presentation layer by exposing a
/// <see cref="ConfigureServices"/> entry point that executes a service registration pipeline.
/// </summary>
/// <remarks>
/// A <see cref="ServiceCompositionRoot{TPipeline}"/> defines the presentation-layer
/// boundary for dependency registration and is intended to be invoked during
/// application startup by a host or bootstrapper that supplies the required
/// <see cref="IServiceCollection"/> and <see cref="IConfiguration"/> instances.
/// <para>
/// This type establishes a clear separation of concerns between the presentation
/// layer and lower application or infrastructure layers. The presentation layer
/// controls when service composition occurs, while all foundational service
/// registration logic is delegated to a strongly typed
/// <see cref="ServiceRegistrationPipeline"/>.
/// </para>
/// <para>
/// By delegating service registration to a pipeline, infrastructure and application
/// layers define a stable, ordered set of registrations that are always executed
/// before any presentation-specific services. This guarantees that core
/// infrastructure dependencies are registered consistently and cannot be
/// interleaved with or controlled by the presentation layer, which is limited to
/// layering its own services on top of the pipeline.
/// </para>
/// </remarks>
/// <typeparam name="TPipeline">
/// The <see cref="ServiceRegistrationPipeline"/> implementation that defines the
/// ordered set of service registrations to execute.
/// </typeparam>
public abstract class ServiceCompositionRoot<TPipeline> : ServiceRegistrationExpressionCollection
    where TPipeline : ServiceRegistrationPipeline, new()
{
    /// <summary>
    /// Configures application services by executing a service registration pipeline
    /// using the provided service collection and configuration.
    /// </summary>
    /// <remarks>
    /// This method is intended to be invoked during application startup by a host or
    /// bootstrapper that supplies the <see cref="IServiceCollection"/> and
    /// <see cref="IConfiguration"/> instances.
    /// <para>
    /// The method creates a new instance of the configured
    /// <see cref="ServiceRegistrationPipeline"/>, populates it with any
    /// presentation-layer service registrations defined on this composition root,
    /// and then executes the pipeline. Infrastructure-level registrations defined
    /// by the pipeline are always executed before presentation-specific services.
    /// </para>
    /// </remarks>
    /// <param name="serviceCollection">
    /// The <see cref="IServiceCollection"/> to which services will be registered.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance used to supply configuration values
    /// to service registrations that require them.
    /// </param>
    public void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var pipeline = new TPipeline();
        pipeline.AddRegistrations(Expressions);
        pipeline.Execute(serviceCollection, configuration);
    }
}