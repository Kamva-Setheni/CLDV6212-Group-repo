using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.Configure<WorkerOptions>(options =>
    options.Serializer = new JsonObjectSerializer(MenuJson.Options));

builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IDocumentStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AzureWebJobsStorage"]
        ?? throw new InvalidOperationException("AzureWebJobsStorage connection string is missing.");
    return new DocumentStorageService(connectionString);
});
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}
builder.Build().Run();
