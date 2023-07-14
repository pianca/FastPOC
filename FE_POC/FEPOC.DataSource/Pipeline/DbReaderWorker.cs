using FEPOC.DataSource.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline;

public class DbReaderWorker : BackgroundService
{
    private readonly ILogger<DbReaderWorker> _logger;
    private readonly TimeSpan _period;
    private readonly DbReaderQueue<ChangedRecordsQueue> _queue;

    public DbReaderWorker(ILogger<DbReaderWorker> logger, DbReaderQueue<ChangedRecordsQueue> queue)
    {
        _logger = logger;
        _period = TimeSpan.FromSeconds(13);
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
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
        var list = await db.ChangedRecordsQueues
            .Where(x => x.LocalSync == "F")
            .OrderBy(x => x.Id)
            .ToListAsync();
        foreach (var item in list)
        {
            _logger.LogInformation("Read a record from db with id {id}", item.Id);
            await _queue.Enqueue(item, stoppingToken);
        }
    }
}