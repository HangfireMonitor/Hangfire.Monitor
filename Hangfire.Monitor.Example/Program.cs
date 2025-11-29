using Hangfire;
using Hangfire.Monitor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();

builder.Services.AddHangfireMonitor(builder.Configuration["HangfireMonitor:ApiKey"]);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();