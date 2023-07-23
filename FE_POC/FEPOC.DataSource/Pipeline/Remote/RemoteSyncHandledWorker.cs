using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline.Remote;

public class RemoteSyncHandledWorker : BackgroundService
{
    private readonly ILogger<RemoteSyncHandledWorker> _logger;
    private readonly RemoteHandledChangesQueue _inputQueue;

    public RemoteSyncHandledWorker(ILogger<RemoteSyncHandledWorker> logger,
            RemoteHandledChangesQueue inputQueue)
    {
        _logger = logger;
        _inputQueue = inputQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RemoteSyncHandledWorker Started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {                
                await Task.Delay(1000);
                await DoStuff(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException &&
                                      e is not TaskCanceledException)
            {
                _logger.LogError(e, "RemoteSyncHandledWorker Exception catched");
            }
        }
    }

    private async Task DoStuff(CancellationToken stoppingToken)
    {
        using var db = new DAL.SqlServer.DB();
        var list = _inputQueue.GetConsumingEnumerable(stoppingToken);
        foreach (var changeId in list)
        {
            bool done = false;
            do
            {
                try
                {
                    // if (db == null)
                    // {
                    //     db = new DAL.SqlServer.DB();
                    // }

                    var rowNum = await db.ChangedRecordsQueues
                        .Where(x => x.Id == changeId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.RemoteSync, p => "T"));
                    if (rowNum == 1)
                    {
                        _logger.LogInformation("Set change with id {id} as remotly handled ", changeId);
                        done = true;
                        // _outputQueue.Add(changeId);
                    }
                }
                catch (Exception e)when (e is not OperationCanceledException &&
                                         e is not TaskCanceledException)
                {
                    _logger.LogError(e, $"Set change with id {changeId} as remotly handled failed");
                    // db = null;
                }
            } while (done == false);
        }
    }
}