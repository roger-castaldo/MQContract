**OpenTelemetry**

This project includes lightweight instrumentation that integrates with OpenTelemetry. Enabling it causes MQContract to emit Activity spans for connection operations (publish, query, subscribe, etc.) and — when enabled — to propagate tracing context across message headers so related work can be linked across services.

## Quick start

1. Add and configure OpenTelemetry in your application (choose the exporters you need):

```csharp
// Program.cs / Startup.cs
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetryTracing(b =>
{
	b
	 .AddSource("MQContract") // ensure this matches the activitySource you pass below
	 .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
	 .AddAspNetCoreInstrumentation()
	 .AddHttpClientInstrumentation()
	 .AddConsoleExporter(); // or AddJaegerExporter(), AddOtlpExporter(), AddZipkinExporter(), etc.
});
```

2. Enable OpenTelemetry on your `ContractConnection` (or other connection instances):

```csharp
// after you create or obtain the connection instance
contractConnection.EnableOpenTelemetry(activitySource: "MQContract", linkActivitiesAcrossSystems: true);
```

- `activitySource` (default: `MQContract`): the ActivitySource name your OpenTelemetry tracer listens to. Make sure the name you register with `AddSource(...)` matches this value.
- `linkActivitiesAcrossSystems` (default: `true`): when set, MQContract will propagate tracing metadata in message headers to allow tracing systems to correlate work across services.

## Notes and recommendations

- Configure an exporter (Jaeger / OTLP / Zipkin / Console) in your application so spans are sent to a back end.
- If you set a custom `activitySource` name, call `AddSource("YourName")` when configuring OpenTelemetry.
- Linking relies on propagating standard trace context (traceparent/tracestate/baggage) via message headers; ensure downstream services read those headers or also enable the MQContract tracing features there.

## Troubleshooting

- No spans visible: confirm your app configured an exporter and that `AddSource` includes the `activitySource` you passed to `EnableOpenTelemetry`.
- Context not linked across services: confirm `linkActivitiesAcrossSystems` is `true` and that downstream services propagate/consume the standard trace headers.

## Example output

Below is a sample tracing view taken while running a Query/Response scenario (linking enabled):

![Sample Query Response Output](images/open_telemetry.png)