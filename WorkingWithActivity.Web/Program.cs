using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder
        .AddService(serviceName: Instrumentation.ActivitySourceName, serviceVersion: Instrumentation.ActivitySourceName))
    .WithTracing(traceBuilder => traceBuilder
        .AddSource(Instrumentation.ActivitySourceName)
        .SetSampler(new AlwaysOnSampler()) // Crucial to have this line to enable tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddZipkinExporter())
    .WithMetrics(metricsBuilder => metricsBuilder
        .AddMeter(Instrumentation.ActivitySourceName)
        .AddConsoleExporter());

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder
        .CreateDefault()
        .AddService(serviceName: Instrumentation.ActivitySourceName, serviceVersion: Instrumentation.ActivitySourceName));
    options.AddConsoleExporter();
});

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<Instrumentation>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var activitySource = context.RequestServices.GetRequiredService<Instrumentation>().ActivitySource;
    using (var activity = activitySource.StartActivity("Middleware1", ActivityKind.Internal))
    {
        activity?.SetTag("Name", "Middleware1");
        activity?.SetTag("Path", context.Request.Path.Value);

        await Task.Delay(100); // Simulate some work
        activity?.SetStatus(ActivityStatusCode.Ok, "");
    }


    await next(context);
});

app.Use(async (context, next) =>
{
    var activitySource = context.RequestServices.GetRequiredService<Instrumentation>().ActivitySource;
    using (var activity = activitySource.StartActivity("Middleware1", ActivityKind.Internal))
    {
        activity?.SetTag("Name", "Middleware1");
        activity?.SetTag("Path", context.Request.Path.Value);

        await Task.Delay(100); // Simulate some work
        activity?.SetStatus(ActivityStatusCode.Ok, "");
    }

    await next(context);
});

app.UseHealthChecks("/health", new HealthCheckOptions { AllowCachingResponses = false });

app.MapGet("/", () => "Working with Traces and OpenTelemetry!")
    .WithName("Home")
    .WithDescription("Home route")
    .WithDisplayName("Home");

app.Run();

public class Instrumentation : IDisposable
{
    public const string ActivitySourceName = "WorkingWithActivity.Web";
    public const string ActivitySourceVersion = "1.0.0";

    public ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName, ActivitySourceVersion);

    public void Dispose() => ActivitySource.Dispose();
}
