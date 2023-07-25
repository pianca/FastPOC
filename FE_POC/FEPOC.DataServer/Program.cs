using FastEndpoints;
using FastEndpoints.Swagger;
using FEPOC.Contracts;
using FEPOC.DataServer.Handlers;
using FEPOC.Models.InMemory;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders()
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information);
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenLocalhost(6000, o => o.Protocols = HttpProtocols.Http2); // for GRPC
    o.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http1AndHttp2); // for REST
    o.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http1AndHttp2); // for REST
});
builder.AddHandlerServer();

builder.Services.AddSingleton<InMemoryState>();
// builder.Services.AddHostedService<TestWorker>();

//gui
builder.Services.AddFastEndpoints();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerDoc(
        s => s.DocumentName = "MyApi",
        shortSchemaNames: true,
        removeEmptySchemas: true);
}

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
//
// app.UseBlazorFrameworkFiles();
// app.UseStaticFiles();
// app.MapFallbackToFile("index.html");
// app.UseRouting();
// app.UseAuthorization();
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



//NOTE: just run `dotnet run --generateclients true` anytime you wanna update the ApiClient in the FEPOC.GUI project

// await app.GenerateClientsAndExitAsync(
//     documentName: "MyApi",
//     destinationPath: "../Client/HttpClient",
//     csSettings: c =>
//     {
//         c.ClassName = "ApiClient";
//         c.CSharpGeneratorSettings.Namespace = "FEPOC.GUI";
//         c.CSharpGeneratorSettings.JsonLibrary = CSharpJsonLibrary.SystemTextJson;
//     },
//     tsSettings: null);

app.Run();