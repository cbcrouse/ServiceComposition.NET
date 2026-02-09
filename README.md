# ServiceComposition.NET

[![Main Status](https://github.com/cbcrouse/ServiceComposition.NET/actions/workflows/dotnet.main.status.yml/badge.svg)](https://github.com/cbcrouse/ServiceComposition.NET/actions/workflows/dotnet.main.status.yml) [![NuGet Downloads](https://img.shields.io/nuget/dt/ServiceComposition.NET)](https://www.nuget.org/stats/packages/ServiceComposition.NET?groupby=Version) [![NuGet Version](https://img.shields.io/nuget/v/ServiceComposition.NET)](https://www.nuget.org/packages/ServiceComposition.NET) [![codecov](https://codecov.io/gh/cbcrouse/ServiceComposition.NET/branch/main/graph/badge.svg?token=XVPL3HNHDG)](https://codecov.io/gh/cbcrouse/ServiceComposition.NET) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cbcrouse_ServiceComposition.NET&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=cbcrouse_ServiceComposition.NET)

## 🔍 Overview

ServiceComposition.NET is a lightweight service composition library for .NET applications that need a clear boundary between **defining service registrations** and **executing them at startup**.

The library introduces two simple building blocks:

- **Service registration pipelines**, which define an ordered set of service registrations  
- **Service composition roots**, which execute those registrations using services and configuration supplied by the host  

This structure enforces a predictable startup flow. Foundational registrations run first, and presentation-specific services are added afterward. That ordering is explicit and consistent, which helps prevent accidental coupling between layers and makes startup behavior easier to understand.

ServiceComposition.NET does **not** own configuration, hosting, or application startup. Those responsibilities remain with the host. The library focuses solely on composing services in a controlled, testable way that works across different application types and hosting models.

The result is startup code that is easier to understand, easier to test, and easier to maintain as applications grow.

---

## ✅ Features

- Defines an explicit, ordered pipeline for service registrations, ensuring foundational services are always registered before presentation-specific services.
- Keeps service registration logic independent of any hosting model, startup pattern, or presentation framework.
- Allows application and infrastructure layers to register services without referencing `Program`, `Startup`, or other presentation-layer constructs.
- Uses expression-based service registrations that are validated when added and executed later during startup.
- Supports both configuration-aware and configuration-free registrations without forcing all registrations to depend on `IConfiguration`.
- Leaves ownership of `IServiceCollection` and `IConfiguration` with the host, keeping startup behavior explicit and predictable.
- Emits detailed startup logging for each registration, including execution order and failures.
- Works anywhere `IServiceCollection` and `IConfiguration` are used for service registration in a .NET application.
- Improves testability by allowing service composition to be executed directly against a service collection without starting a full host.

---

## 👤 Audience

ServiceComposition.NET is intended for .NET developers who want clearer structure and stronger boundaries around service registration as their applications grow.

It is especially useful when startup logic begins to spread across `Program.cs`, extension methods, and infrastructure projects, making it harder to understand what is registered, in what order, and under which conditions.

This library is a good fit if you:

- Maintain multiple applications or hosts that share a common application or infrastructure layer  
- Want to keep service registration logic out of presentation-specific startup code  
- Prefer explicit, testable startup composition over large or implicit startup methods  
- Need visibility into registration order and early startup failures  

ServiceComposition.NET works well for web applications, minimal APIs, background services, console applications, and test hosts—anywhere `IServiceCollection` and `IConfiguration` are used.

It does not require Clean Architecture, but it aligns naturally with layered designs where service composition is treated as a boundary concern.

For very small or short-lived applications, the additional structure may not be necessary. For systems that evolve over time or support multiple entry points, the library provides a clear and maintainable way to organize startup logic without coupling it to a specific hosting model.

---

## 📦 Getting Started

You can install the package via NuGet. Search for `ServiceComposition.NET` or run the following command:

```sh
dotnet add package ServiceComposition.NET
```

---

## 🧑‍💻 Usage

ServiceComposition.NET is built around a simple idea:

**service registration should be explicit, ordered, testable, and independent of the hosting model.**

Instead of scattering service registrations across `Program.cs`, extension methods, and framework-specific startup hooks, this library allows you to define registrations as reusable pipelines that can be executed by any presentation layer supplying an `IServiceCollection` and an `IConfiguration`.

A clean architecture structure is not required, but it provides a helpful mental model for how pipelines and composition roots fit together.

---

### Example Project Structure

```text
MyProject/
├─ src/
│  ├─ Application/
│  │  ├─ MyApp.Application.csproj
│  │  └─ Services/
│  │     └─ MyAppService.cs
│  ├─ Infrastructure/
│  │  ├─ MyApp.Infrastructure.csproj
│  │  ├─ Persistence/
│  │  │  ├─ MyDbContext.cs
│  │  │  └─ Repositories/
│  │  │     └─ MyRepository.cs
│  │  └─ Startup/
│  │     └─ InfrastructureRegistrationPipeline.cs
│  ├─ Presentation/
│  │  ├─ MyApp.Api/
│  │  │  ├─ MyApp.Api.csproj
│  │  │  ├─ Program.cs
│  │  │  └─ WebApiServiceComposition.cs
│  │  ├─ MyApp.Console/
│  │  │  ├─ MyApp.Console.csproj
│  │  │  ├─ Program.cs
│  │  │  └─ ConsoleServiceComposition.cs
│  │  └─ ...
└─ tests/
   ├─ MyApp.UnitTests/
   ├─ MyApp.IntegrationTests/
   └─ ...
```

In this structure:

- **Infrastructure** defines a service registration pipeline
- **Presentation** owns startup and execution
- The infrastructure pipeline is reusable across multiple presentation layers
- No pipeline depends on a specific hosting model

---

### Core Concepts

There are two main building blocks:

- **ServiceRegistrationPipeline**

  A reusable, ordered set of validated service registration expressions.

- **ServiceCompositionRoot**

  The presentation-layer entry point that executes the pipeline using the provided services and configuration.

Each registration is expressed as:

```csharp
Expression<Action<IServiceCollection, IConfiguration>>
```

This allows ServiceComposition.NET to:

- Validate registrations up front  
- Execute them in a predictable order  
- Log each registration before and after execution  
- Surface failures with clear startup context  

---

### Defining a Pipeline

Pipelines define *what* services should be registered, without assuming *where* or *how* the application is hosted.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class CoreServiceRegistrationPipeline : ServiceRegistrationPipeline
{
    protected override ILogger StartupLogger => NullLogger.Instance;

    public CoreServiceRegistrationPipeline()
    {
        AddRegistration((services, config) =>
            services.RegisterConfiguredOptions<MyOptions>(config));

        AddRegistration((services, _) =>
            services.AddSingleton<IMyService, MyService>());
    }
}
```

Each call to `AddRegistration` adds a validated service registration expression to the pipeline. Invalid expressions are rejected immediately.

---

### Defining a Service Composition Root

A service composition root is a presentation-layer class that executes a service registration pipeline and adds any presentation-specific registrations.

To define one, inherit from `ServiceCompositionRoot<TPipeline>` and add registrations in the constructor.

```csharp
using Microsoft.Extensions.DependencyInjection;

public sealed class WebAppServiceComposition
    : ServiceCompositionRoot<CoreServiceRegistrationPipeline>
{
    public WebAppServiceComposition()
    {
        AddRegistration(services =>
            services.AddAuthorization());

        AddRegistration(services =>
            services.AddEndpointsApiExplorer());
    }
}
```

The generic pipeline type (`CoreServiceRegistrationPipeline`) defines the shared registrations that always run first. Any registrations added here are applied afterward.

---

### Composing Services in the Presentation Layer

The presentation layer controls execution. It supplies the `IServiceCollection` and `IConfiguration`, and decides which pipelines to run.

This example uses a minimal API, but the same approach works for web APIs, background services, console apps, and test hosts.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["My:Property"] = "Value"
});

var compositionRoot = new WebAppServiceComposition();
compositionRoot.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello world");

app.Run();
```

The pipelines do not create or manage configuration or services. They simply consume what the presentation layer provides.

---

### Reusing Pipelines Across Hosts

Because pipelines rely only on `IServiceCollection` and `IConfiguration`, the same pipelines can be reused across multiple entry points:

- Web APIs
- Minimal APIs
- Background services
- Console applications
- Integration tests

This avoids duplicated startup logic and keeps infrastructure consistent and decoupled from presentation dependency concerns.

---

### Using Pipelines in Tests

Pipelines can be executed directly in tests without spinning up a full host.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["My:Property"] = "TestValue"
    })
    .Build();

var pipeline = new CoreServiceRegistrationPipeline();
pipeline.Execute(services, configuration);

var provider = services.BuildServiceProvider();
var service = provider.GetRequiredService<IMyService>();
```

This makes startup behavior easy to validate and understand in isolation.

---

### Summary

- Pipelines define *what* gets registered  
- The presentation layer controls *when* and *with which configuration*  
- Registrations are validated, ordered, and logged  
- No inheritance-based startup model  
- No coupling to a specific hosting framework  

ServiceComposition.NET fits naturally anywhere `IServiceCollection` and `IConfiguration` are already in use.

---

## 🔌 ServiceRegistrationPipeline and ServiceCompositionRoot

ServiceComposition.NET is built around two core concepts: **pipelines** and **composition roots**.

### ServiceRegistrationPipeline

A `ServiceRegistrationPipeline` defines an ordered set of service registration expressions.  
It is typically implemented outside the presentation layer (for example, in an application or infrastructure project) and contains registrations that should **always run first**, regardless of how the application is hosted.

The pipeline does not create or manage `IServiceCollection` or `IConfiguration`. Instead, it declares *what* should be registered and relies on the presentation layer to supply those objects at execution time.

This makes pipelines reusable across different hosts and ensures that foundational services are registered consistently.

### ServiceCompositionRoot

A `ServiceCompositionRoot<TPipeline>` represents the presentation-layer entry point for service composition.

Its responsibility is to:
- Accept the `IServiceCollection` and `IConfiguration` provided by the host
- Execute the referenced `ServiceRegistrationPipeline`
- Add any presentation-specific service registrations on top

By design, the presentation layer never controls *when* pipeline registrations occur—only *what* it adds afterward. This guarantees that core infrastructure and application services are registered first and cannot be interleaved with presentation concerns.

Together, these two types create a clear boundary:
- Pipelines define **shared, ordered registrations**
- Composition roots define **host-specific additions**

This separation keeps startup logic explicit, predictable, and easy to reuse across different application types.

---

## 📄 The Use of Expressions

ServiceComposition.NET uses **expression trees** to represent service registrations instead of executing registration logic immediately.

Each service registration is captured as an  
`Expression<Action<IServiceCollection, IConfiguration>>`  
and stored in an ordered collection. These expressions are **not executed when they are added**. They are only compiled and invoked later, when the pipeline is executed by the presentation layer.

This approach provides a few important benefits.

### Deferred execution

Because registrations are stored as expressions, nothing is registered until the pipeline runs. This avoids timing issues that can arise when services are registered across multiple projects or startup paths, and it keeps startup behavior predictable and explicit.

The presentation layer decides *when* service composition happens by calling `Execute`, while the pipeline defines *what* gets registered.

### Ordered, inspectable registrations

Expressions preserve the structure of the registration call itself. This makes it possible to:
- Enforce validation rules (only valid `IServiceCollection` extension methods are allowed)
- Execute registrations in a guaranteed order
- Log each registration with meaningful, human-readable output

This is especially helpful when diagnosing startup failures or unexpected service lifetimes.

### Configuration-aware by default

All registrations are normalized to accept both:
- `IServiceCollection`
- `IConfiguration`

Registrations that don’t require configuration are automatically lifted into this shape. This keeps the execution model consistent while still allowing simple registrations when configuration isn’t needed.

Because the configuration object is supplied by the presentation layer at execution time, pipelines remain independent of any specific hosting model or startup mechanism.

### Clean separation of responsibilities

Using expressions allows lower layers to declare service registrations without:
- Owning the DI container
- Owning configuration
- Depending on a specific presentation framework

The result is a clean, explicit service composition flow where:
- Pipelines define registrations
- Composition roots execute them
- Hosts control startup timing

This keeps startup logic modular, testable, and easy to reason about as applications grow.

### Startup Logging

ServiceComposition.NET includes first-class support for **startup-time logging** through the `StartupLogger` property on a service registration pipeline.

Service registrations often run **before the application’s logging infrastructure is fully configured**. The `StartupLogger` exists specifically to cover this gap, giving you visibility into what happens during service composition—when failures are otherwise difficult to diagnose.

As each registration expression is executed, the pipeline emits trace-level log entries that indicate:
- When a registration starts
- When it completes successfully
- When it fails due to an exception

This makes it easy to see the exact order in which services are registered and to pinpoint the registration that caused a startup failure.

You can back `StartupLogger` with any logging implementation you prefer, including Serilog, NLog, or the built-in logging abstractions. Logs can be written to the console, files, or external systems, depending on how early diagnostics need to be captured.

Startup logging is especially useful when:
- Diagnosing failures that occur before the host fully starts
- Verifying registration order across multiple layers
- Tracking down misconfigured options or missing dependencies

By making startup behavior observable, the library helps reduce guesswork and shortens the feedback loop when something goes wrong during application initialization.

#### Example Log Output

```log
[2024:10:13 07:16:48.999 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddOptions(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.043 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddOptions(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.045 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsFactory`1[TOptions]>)"' was started...
[2024:10:13 07:16:49.046 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsFactory`1[TOptions]>)"' completed successfully!
[2024:10:13 07:16:49.048 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsMonitor`1[TOptions]>)"' was started...
[2024:10:13 07:16:49.049 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsMonitor`1[TOptions]>)"' completed successfully!
[2024:10:13 07:16:49.051 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddOptions<SqlServerOptions>(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.061 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddOptions<SqlServerOptions>(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.064 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddSqlServer(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.067 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddSqlServer(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.170 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddAuthorization(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.181 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddAuthorization(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.184 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddRazorPagesWithAuthorization(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.262 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddRazorPagesWithAuthorization(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.264 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddCascadingAuthenticationState(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.265 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddCascadingAuthenticationState(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.267 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddServerSideBlazor(this IServiceCollection, Action`1)"' was started...
[2024:10:13 07:16:49.293 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddServerSideBlazor(this IServiceCollection, Action`1)"' completed successfully!
[2024:10:13 07:16:49.294 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddMudBlazorServices(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.303 PM] [Verbose] [CoreServiceRegistrationPipeline] '"AddMudBlazorServices(this IServiceCollection, IConfiguration)"' completed successfully!
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7206
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5035
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

---

## ✔ Writing Valid Service Registration Expressions

Every service registration expression added to a pipeline is **validated at the point it’s added**, not when the pipeline runs. This early validation step ensures that only legitimate service registration calls make it into the execution pipeline.

A valid service registration expression must meet two criteria:
- It must be a **method call expression**
- The method must be an **extension method declared on `IServiceCollection`**

This guarantees that the pipeline only contains actual DI registrations and prevents subtle runtime failures caused by accidental logic, assignments, or non-registration expressions slipping in.

If an invalid expression is added, an exception is thrown immediately, keeping failures close to the source and easy to diagnose.

### Examples of valid expressions

```csharp
AddRegistration((services, config) =>
    services.AddTransient<IMyService, MyService>());

AddRegistration((services, config) =>
    services.AddTransient(typeof(IMyService), typeof(MyService)));

AddRegistration((services, config) =>
    services.AddMvcCore());

AddRegistration((services, config) =>
    services.RegisterOptions<MyOptions>(config));
```

Custom extension methods are fully supported, as long as they extend `IServiceCollection`. This makes it easy to encapsulate complex registrations behind well-named helpers while still benefiting from validation and startup logging.

By enforcing these rules consistently, ServiceComposition.NET keeps service composition predictable, explicit, and safe.

---

## Contributing

Contributions are welcome! If you find a bug, want to suggest a feature, or want to contribute code, please open an issue or submit a pull request.

## License

This package is released under the MIT License. See [LICENSE](./LICENSE) for more information.
