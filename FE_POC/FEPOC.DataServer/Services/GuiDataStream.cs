using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using FEPOC.Common;
using FEPOC.Common.DTO;
using FEPOC.Common.InMemory;
using FEPOC.DataServer.Hub;
using Microsoft.AspNetCore.SignalR;

namespace FEPOC.DataServer.Services;

public class GuiDataStream
{
    private readonly IHubContext<GuiHub> _hubContext;
    private readonly InMemoryState _inMemoryState;

    private readonly ILogger<GuiDataStream> _logger;

    //TODO: usa concurrewnt dictionary
    private readonly Dictionary<string, Task> _streamTasks = new Dictionary<string, Task>();

    public GuiDataStream(ILogger<GuiDataStream> logger, IHubContext<GuiHub> hubContext, InMemoryState inMemoryState)
    {
        _logger = logger;
        _hubContext = hubContext;
        _inMemoryState = inMemoryState;
    }

    async Task Create(string connectionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new gui data stream {connectionId}", connectionId);
        var snap = await _inMemoryState.Snapshots
            .Where(x => x is not null)
            .Take(1)
            .FirstOrDefaultAsync();

        _logger.LogInformation("Starting gui data stream to {connectionId} with snapshot id {snapshotId}",
            connectionId,
            snap.Version.Id);
        await Notify(snap, connectionId, cancellationToken);

        var changes = await _inMemoryState.Changes
            .Where(x => x.Info.Id > snap.Version.Id)
            .Do(x =>
                _logger.LogInformation("Sending change with id {changeId} to client {connectionId}",
                    x.Info.Id,
                    connectionId))
            .Select(x => Observable.FromAsync(() => Notify(x, connectionId, cancellationToken)))
            .Concat()
            .ToTask();

        _logger.LogInformation("Gui data stream completed {connectionId}", connectionId);
    }

    //TODO: si può generalizzare
    private async Task<bool> Notify(InMemoryStateSnapshotDTO data, string connectionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _hubContext.Clients.Client(connectionId);
            if (client is null)
            {
                _logger.LogError("Gui Client with {connectionId} is null", connectionId);
                return false;
            }

            await client.SendAsync(Messages.NEW_SNAPSHOT, data);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SendAsync NEW_SNAPSHOT exception to client {connectionId}", connectionId);
            return false;
        }
    }

    //TODO: si può generalizzare
    private async Task<bool> Notify(IRecordChange data, string connectionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _hubContext.Clients.Client(connectionId);
            if (client is null)
            {
                _logger.LogError("Gui Client with {connectionId} is null", connectionId);
                return false;
            }

            await client.SendAsync(Messages.NEW_CHANGE, data);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SendAsync NEW_CHANGE exception to client {connectionId}", connectionId);
            return false;
        }
    }

    public void AddClient(string connectionId)
    {
        if (_streamTasks.ContainsKey(connectionId))
        {
            _logger.LogWarning("Gui client already present {connectionId} that will be disposed", connectionId);
            //todo: usa cancellatiuon token
            var a = _streamTasks[connectionId];
            a.Dispose();
        }

        _streamTasks[connectionId] = Create(connectionId);
        _logger.LogInformation("Gui stream task created for client {connectionId}", connectionId);
    }

    public void RemoveClient(string connectionId)
    {
        if (_streamTasks.ContainsKey(connectionId))
        {
            _streamTasks[connectionId].Dispose();
            _logger.LogInformation("Gui stream task terminated for client {connectionId}", connectionId);
        }
    }
}