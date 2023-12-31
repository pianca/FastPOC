﻿using FastEndpoints;
using FastEndpoints.ClientGen;
using FastEndpoints.Swagger;
using FEPOC.Contracts;
using FEPOC.DataServer.Handlers;
using FEPOC.Common.InMemory;
using FEPOC.DataServer;
using FEPOC.DataServer.Hub;
using FEPOC.DataServer.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using NJsonSchema.CodeGeneration.CSharp;
using Serilog;
using Serilog.Settings.Configuration;

var builder = WebApplication.CreateBuilder();

// builder.Logging.ClearProviders()
//     .AddConsole()
//     .SetMinimumLevel(LogLevel.Information);
var bootstrapConfiguration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#if DEBUG
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
    // .AddJsonFile(.CUSTOM_CONFIG_FILENAME, optional: true, reloadOnChange: true)
    .Build();
var logConfigBuilder = new LoggerConfiguration();
var run = DateTime.Now;
Log.Logger = logConfigBuilder
    .ReadFrom.Configuration(bootstrapConfiguration, "Serilog", ConfigurationAssemblySource.AlwaysScanDllFiles)
    .Enrich.WithProperty("Application", Const.AppName)
    .Enrich.WithProperty("Run", run)
    .CreateBootstrapLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenLocalhost(6000, o => o.Protocols = HttpProtocols.Http2); // for GRPC
    o.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http1AndHttp2); // for REST
    o.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http1AndHttp2); // for REST
});
builder.AddHandlerServer();

builder.Services.AddFastEndpoints();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerDoc(
        s => s.DocumentName = "MyApi",
        shortSchemaNames: true,
        removeEmptySchemas: true);
}

builder.Services.AddSingleton<InMemoryState>();
builder.Services.AddSingleton<GuiDataStream>();

// builder.Services.AddHostedService<TestWorker>();

#region GUI
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
#endregion


var app = builder.Build();

app.MapHandlers(h =>
{
    // h.Register<SayHelloCommand, SayHelloHandler>();

    h.Register<SyncInitCommand, SyncInitHandler, SyncInitResult>();
    h.Register<SyncChangeAreaCommand, SyncChangeAreaHandler, SyncChangeResult>();
    h.Register<SyncChangeInsediamentoCommand, SyncChangeInsediamentoHandler, SyncChangeResult>();

    // h.RegisterServerStream<StatusStreamCommand, StatusUpdateHandler, StatusUpdate>();
    // h.RegisterClientStream<CurrentPosition, PositionProgressHandler, ProgressReport>();
    // h.RegisterEventHub<SomethingHappened>();
});

#region GUI
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.UseRouting();
//app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapHub<GuiHub>("/guihub");
//app.MapHub<ChatHub>("/chathub");
#endregion

app.UseFastEndpoints(c =>
{
    c.Endpoints.ShortNames = true;
    c.Serializer.Options.PropertyNamingPolicy = null;
});


if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseOpenApi();
    app.UseSwaggerUi3(s => s.ConfigureDefaults());
}
else
{
    app.UseExceptionHandler("/Error");
}

// app.MapGet("/event/{name}", async (string name) =>
// {
//     for (int i = 1; i <= 10; i++)
//     {
//         new SomethingHappened
//         {
//             Id = i,
//             Description = name
//         }
//         .Broadcast();
//
//         await Task.Delay(1000);
//     }
//     return Results.Ok("events published!");
// });

//app.UseResponseCompression();

//NOTE: just run `dotnet run --generateclients true` anytime you wanna update the ApiClient in the FEPOC.GUI project

await app.GenerateClientsAndExitAsync(
    documentName: "MyApi",
    destinationPath: "../FEPOC.GUI/HttpClient",
    csSettings: c =>
    {
        c.ClassName = "ApiClient";
        c.CSharpGeneratorSettings.Namespace = "FEPOC.GUI";
        c.CSharpGeneratorSettings.JsonLibrary = CSharpJsonLibrary.SystemTextJson;
    },
    tsSettings: null);

app.Run();
