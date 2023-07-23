using System.Collections.Concurrent;
using FEPOC.DataSource.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline.Local;
public class LocalSyncHandledWorker : BackgroundService
{
    private readonly ILogger<LocalSyncHandledWorker> _logger;
    private readonly LocalHandledChangesQueue _inputQueue;

    public LocalSyncHandledWorker(ILogger<LocalSyncHandledWorker> logger, 
        LocalHandledChangesQueue inputQueue)
    {
        _logger = logger;
        _inputQueue = inputQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LocalSyncHandledWorker Started");
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
                _logger.LogError(e, "LocalSyncHandledWorker Exception catched");
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
                        .ExecuteUpdateAsync(s => s.SetProperty(
                            p => p.LocalSync,
                            p => "T"));
                    if (rowNum == 1)
                    {
                        _logger.LogInformation("Set change with id {id} as locally handled ", changeId);
                        done = true;
                        // _outputQueue.Add(changeId);
                    }
                }
                catch (Exception e)when (e is not OperationCanceledException &&
                                         e is not TaskCanceledException)
                {
                    _logger.LogError(e, $"Set change with id {changeId} as locally handled failed");
                    // db = null;
                }
            } while (done == false);
        }
    }
}