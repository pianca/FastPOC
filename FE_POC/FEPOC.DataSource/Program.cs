using FastEndpoints;
using FEPOC.Contracts;
using FEPOC.DataSource.Pipeline;
using FEPOC.DataSource.Pipeline.Local;
using FEPOC.DataSource.Pipeline.Remote;
using FEPOC.DataSource.Utility;
using FEPOC.Common.InMemory;
using Mapster;

TypeAdapterConfig.GlobalSettings.EnableJsonMapping();
TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders()
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton<ParserErrorQueue>();
builder.Services.AddSingleton<FactoryErrorQueue>();

builder.Services.AddSingleton<InMemoryState>();

builder.Services.AddSingleton<LocalHandledChangesQueue>();
builder.Services.AddSingleton<LocalSyncErrorsQueue>();

builder.Services.AddSingleton<RemoteSyncQueue>();
builder.Services.AddSingleton<RemoteHandledChangesQueue>();
builder.Services.AddSingleton<RemoteSyncErrorsQueue>();

builder.Services.AddSingleton<InMemorySnapshotQueue>();

builder.Services.AddHostedService<LocalSyncWorker>();
builder.Services.AddHostedService<LocalSyncHandledWorker>();

builder.Services.AddHostedService<RemoteSyncWorker>();
builder.Services.AddHostedService<RemoteSyncHandledWorker>();

var app = builder.Build();

//todo: rename to MapRemote()
app.MapRemote("http://localhost:6000", c =>
{
    // c.Register<SayHelloCommand>();
    
    c.Register<SyncChangeInsediamentoCommand, SyncChangeResult>();
    c.Register<SyncChangeAreaCommand, SyncChangeResult>();
    c.Register<SyncInitCommand, SyncInitResult>();
    
    // c.RegisterServerStream<StatusStreamCommand, StatusUpdate>();
    // c.RegisterClientStream<CurrentPosition, ProgressReport>();
    // c.Subscribe<SomethingHappened, WhenSomethingHappens>();
});
/*
//VOID TEST
app.MapGet("/", async () =>
{
    await new SayHelloCommand
    {
        From = "mars"
    }
    .RemoteExecuteAsync();

    return Results.Ok();
});

//UNARY TEST
app.MapGet("/{id}", async (int id) =>
{
    var result = await new CreateOrderCommand
    {
        OrderId = id,
        CustomerName = "Holly Simms"
    }
    .RemoteExecuteAsync();

    return Results.Ok("Result from remote handler: " + result.Message);
});

//SERVER STREAM TEST
app.MapGet("/server-stream/{id}", async (int id, HttpContext ctx) =>
{
    try
    {
        var iterator = new StatusStreamCommand
        {
            Id = id,
        }
        .RemoteExecuteAsync(new(cancellationToken: new CancellationTokenSource(5000).Token));

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "application/json"; //just so the web browser will render the chunks
        await ctx.Response.StartAsync();

        await foreach (var res in iterator)
            await ctx.Response.WriteAsync(res.Message + Environment.NewLine + Environment.NewLine);
    }
    catch (OperationCanceledException) { }
    catch (RpcException) { }
});

//CLIENT STREAM TEST
app.MapGet("/client-stream", async (CancellationToken ct) =>
{
    var report = await GetDataStream(ct)
             .RemoteExecuteAsync<CurrentPosition, ProgressReport>(new(cancellationToken: ct));

    return Results.Ok(report);

    static async IAsyncEnumerable<CurrentPosition> GetDataStream([EnumeratorCancellation] CancellationToken ct)
    {
        var i = 0;
        while (!ct.IsCancellationRequested && i < 5)
        {
            i++;
            yield return new() { Number = i };
            await Task.Delay(1000, ct);
        }
    }
});
*/

app.Run();


