using System.Collections.Concurrent;
using FEPOC.DataSource.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline;

public class SetChangesAsLocalHandledWorker : BackgroundService
{
    private readonly ILogger<SetChangesAsLocalHandledWorker> _logger;
    private readonly BlockingCollection<int> _queue;
    private readonly TimeSpan _period;

    public SetChangesAsLocalHandledWorker(ILogger<SetChangesAsLocalHandledWorker> logger, RemoteHandledChangesQueue queue)
    {
        _logger = logger;
        _queue = queue;
        _period = TimeSpan.FromSeconds(5); //ChangeHandledLocalQueue
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // using PeriodicTimer timer = new(_period);
        while (
                !stoppingToken.IsCancellationRequested) //&&
            // await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await DoStuff(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException &&
                                      e is not TaskCanceledException)
            {
                _logger.LogError(e, "Exception catched");
            }
        }
    }

    private async Task DoStuff(CancellationToken stoppingToken)
    {
        using var db = new DAL.SqlServer.DB();
        var list = _queue.GetConsumingEnumerable(stoppingToken);
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