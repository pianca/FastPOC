using FEPOC.Models.InMemory;

namespace FEPOC.DataServer;
public class TestWorker : BackgroundService
{
    private readonly ILogger<TestWorker> _logger;
    private readonly TimeSpan _period;
    private readonly InMemoryState _inMemoryState;

    public TestWorker(ILogger<TestWorker> logger, InMemoryState inMemoryState)
    {
        _logger = logger;
        _period = TimeSpan.FromSeconds(30);
        _inMemoryState = inMemoryState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000);
        using PeriodicTimer timer = new(_period);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // if (isFirstRun)
                // {
                //     if (await ComputeStartingPoint(stoppingToken))
                //     {
                //         isFirstRun = false;
                //         _logger.LogInformation("ComputeStartingPoint done");
                //     }
                //     else
                //     {
                //         _logger.LogError("ComputeStartingPoint problem. Will retry in a few moment");
                //         continue;
                //     }
                // }

                // await PipelineProcess(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException &&
                                      e is not TaskCanceledException)
            {
                _logger.LogError(e, "Exception catched");
            }
        }
    }

    private Task PipelineProcess(CancellationToken stoppingToken)
    {
        // _inMemoryState.PrintState();
        
        
        
        return Task.CompletedTask;
    }

 
}