
using Microsoft.ApplicationInsights.Extensibility;
using OrderApi.Enrichers;
using OrderApi.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

// ── Bootstrap logger (captures startup errors before config loads) ─
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OrderApi");

    var builder = WebApplication.CreateBuilder(args);

    // ── Replace default logger with Serilog ────────────────────────
    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)   // reads appsettings.json
        .ReadFrom.Services(services)                  // allows DI in sinks
        .Enrich.FromLogContext()                      // LogContext.PushProperty
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.With(new UserContextEnricher(services.GetRequiredService<IHttpContextAccessor>()))// Custom enricher to capture user info from HttpContext
        .WriteTo.Seq("http://localhost:5341",restrictedToMinimumLevel: LogEventLevel.Verbose)// Seq sink with different levels for testing
        .WriteTo.Seq("http://localhost:5341",restrictedToMinimumLevel: LogEventLevel.Debug)
        .WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Information)
        );
        //.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
        //.WriteTo.File(new CompactJsonFormatter(), "logs/app-.json")
        //.WriteTo.ApplicationInsights(
        //    services.GetRequiredService<TelemetryConfiguration>(),
        //    TelemetryConverter.Traces));


    builder.Services.AddControllers();
    //builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddScoped<IOrderService, OrderService>();


    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // ── Serilog HTTP request logging (replaces default middleware) ──
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diag, http) =>
        {
            diag.Set("RequestHost", http.Request.Host.Value);
            diag.Set("UserAgent",   http.Request.Headers["User-Agent"]);
        };
    });

    app.UseMiddleware<CorrelationMiddleware>();

    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "OrderApi terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();  // always flush before exit
}