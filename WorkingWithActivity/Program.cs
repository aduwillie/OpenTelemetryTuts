using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

/**
 * 
 * General Notes
 * 
 * OpenTelemetry uses 'Tracer' and 'Span'
 * .NET uses 'ActivitySource' and 'Activity'
 * 
 * Tracer = ActivitySource
 * Span = Activity
 * 
 * Events are not captured in OpenTelemetry, but they are captured in Activity
 * Better to capture events as logs
 * 
 */

var _ = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder
        .CreateDefault()
        .AddService(serviceName: "WorkingWithActivity", serviceVersion: "1.0.0"))
    .AddSource(Telemetry.ActivitySourceName)
    .AddConsoleExporter()
    .AddZipkinExporter()
    .Build();

await DoWork("firstInput", "secondInput");
Console.WriteLine("Work completed");
Console.ReadLine();

static async Task DoWork(string input1, string input2)
{
    using var activity = Telemetry.source.StartActivity(name: "DoWork", kind: ActivityKind.Internal);
    activity?.SetTag("input1", input1);
    activity?.SetTag("input2", input2);

    await StepOne();
    activity?.AddEvent(new ActivityEvent("Step one completed"));

    await StepTwo();
    activity?.AddEvent(new ActivityEvent("Step two completed"));

    await StepThree();
    activity?.AddEvent(new ActivityEvent("Step three completed"));

    // Indicate that the activity has completed successfully
    activity?.SetStatus(ActivityStatusCode.Ok, "All steps completed successfully");
}

static async Task StepOne()
{
    using var activity = Telemetry.source.StartActivity(name: "StepOne", kind: ActivityKind.Internal);
    await Task.Delay(500);

    // Indicate that the activity has completed successfully
    activity?.SetStatus(ActivityStatusCode.Ok, "Step one completed successfully");
}

static async Task StepTwo()
{
    using var activity = Telemetry.source.StartActivity(name: "StepTwo", kind: ActivityKind.Internal);
    await Task.Delay(500);

    // Indicate that the activity has completed successfully
    activity?.SetStatus(ActivityStatusCode.Ok, "Step two completed successfully");
}

static async Task StepThree()
{
    using var activity = Telemetry.source.StartActivity(name: "StepThree", kind: ActivityKind.Internal);
    await Task.Delay(500);

    // Indicate that the activity has completed successfully
    activity?.SetStatus(ActivityStatusCode.Ok, "Step three completed successfully");
}

class Telemetry
{
    internal const string ActivitySourceName = "TraceInternals.WorkingWithActivity";
    internal static ActivitySource source = new ActivitySource(ActivitySourceName);
    

}
