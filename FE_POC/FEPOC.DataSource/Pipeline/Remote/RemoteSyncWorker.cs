using FastEndpoints;
using FEPOC.Contracts;
using FEPOC.Common.DTO;

namespace FEPOC.DataSource.Pipeline.Remote;

public class RemoteSyncWorker : BackgroundService
{
    private readonly ILogger<RemoteSyncWorker> _logger;
    private readonly TimeSpan _period;
    private readonly RemoteSyncQueue _inputQueue;
    private readonly RemoteHandledChangesQueue _outputQueue;
    private readonly InMemorySnapshotQueue _inMemorySnapshotQueue;

    public RemoteSyncWorker(ILogger<RemoteSyncWorker> logger,
        InMemorySnapshotQueue inMemorySnapshotQueue,
        RemoteSyncQueue queue,
        RemoteHandledChangesQueue remoteHandledChangesQueue)
    {
        _logger = logger;
        _period = TimeSpan.FromSeconds(5);
        _inMemorySnapshotQueue = inMemorySnapshotQueue;
        _inputQueue = queue;
        _outputQueue = remoteHandledChangesQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RemoteSyncWorker Started");
        bool isFirstRun = true;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_period);
            
            try
            {
                if (isFirstRun)
                {
                    if (await SyncInit(stoppingToken))
                    {
                        isFirstRun = false;
                        _logger.LogInformation("Remote SyncInit done");
                    }
                    else
                    {
                        _logger.LogError("Remote SyncInit problem. Will retry in a few moment");
                        continue;
                    }
                }

                await SyncChanges(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException &&
                                      e is not TaskCanceledException)
            {
                _logger.LogError(e, "RemoteSyncWorker Exception catched");
            }
        }
    }

    private async Task<bool> SyncInit(CancellationToken stoppingToken)
    {
        var snapshot = _inMemorySnapshotQueue.Take(stoppingToken);
        int tryNum = 0;
        bool done = false;
        do
        {
            try
            {
                tryNum++;
                var result = await new SyncInitCommand()
                    {
                        Snapshot = snapshot
                    }
                    .RemoteExecuteAsync();

                if (result.Success == false)
                {
                    _logger.LogWarning(
                        "SyncInit FAILED remotely at try {tryNum} with error {commandError}",
                        tryNum,
                        result.Message);
                    continue;
                }
                
                _logger.LogInformation("SyncInit SUCCESS remotely with {tryNum} try", tryNum);
                done = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SyncInit Exception catched");
            }
        } while (done == false);
        return true;
    }

    private async Task SyncChanges(CancellationToken stoppingToken)
    {
        var list = _inputQueue.GetConsumingEnumerable(stoppingToken);
        foreach (var changeRecord in list)
        {
            int tryNum = 0;
            bool done = false;
            do
            {
                try
                {
                    tryNum++;

                    ICommand<SyncChangeResult> command = CreateChangeRecordCommand(changeRecord);
                    
                    //push command to remote server
                    var result = await command.RemoteExecuteAsync();

                    if (result.Success == false)
                    {
                        _logger.LogWarning(
                            "SyncChanges FAILED with id {id} remotely at try {tryNum} with error {commandError}",
                            changeRecord.Info.Id,
                            tryNum,
                            result.Message);
                        await Task.Delay(5000);
                        continue;
                    }

                    _logger.LogInformation("SyncChanges SUCCESS with id {id} remotely with {tryNum} try",
                        changeRecord.Info.Id,
                        tryNum);
                    _outputQueue.Add(changeRecord.Info.Id);
                    done = true;
                }
                catch (Exception e)when (e is not OperationCanceledException &&
                                         e is not TaskCanceledException)
                {
                    _logger.LogError(e, "SyncChanges EXCEPTION with id {id} remotely at try {tryNum}",
                        changeRecord.Info.Id,
                        tryNum);
                }
            } while (done == false);
        }
    }

    private ICommand<SyncChangeResult> CreateChangeRecordCommand(IRecordChange changeRecord)
    {
        if (changeRecord is ChangedArea)
            return new SyncChangeAreaCommand{RecordChange = changeRecord as ChangedArea};
        if (changeRecord is ChangedInsediamento)
            return new SyncChangeInsediamentoCommand{RecordChange = changeRecord as ChangedInsediamento};

        throw new NotSupportedException($"The record change type {changeRecord.GetType().Name} is not supported");
    }
}