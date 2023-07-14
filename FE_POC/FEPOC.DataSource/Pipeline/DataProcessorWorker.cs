using FEPOC.DataSource.DAL.Models;

namespace FEPOC.DataSource.Pipeline;

public class DataProcessorWorker : BackgroundService
{
    private readonly ILogger<DataProcessorWorker> _logger;
    private readonly DbReaderQueue<ChangedRecordsQueue> _queue;
    private readonly TimeSpan _period;

    public DataProcessorWorker(ILogger<DataProcessorWorker> logger, DbReaderQueue<ChangedRecordsQueue> queue)
    {
        _logger = logger;
        _queue = queue;
        _period = TimeSpan.FromSeconds(5);
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
        var changedRecord = await _queue.DequeueSingle(stoppingToken);
        var value = Parser.Parse(changedRecord);
        _logger.LogInformation("Parsed a record with id {id} of type {type}", 
            changedRecord.Id, value.GetType().Name);
    }
    
}