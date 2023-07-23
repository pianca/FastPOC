using FEPOC.DataSource.DAL.Models;
using FEPOC.DataSource.Pipeline.Remote;
using FEPOC.DataSource.Utility;
using FEPOC.Models.DTO;
using FEPOC.Models.InMemory;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline.Local;

public class LocalSyncWorker : BackgroundService
{
    private readonly ILogger<LocalSyncWorker> _logger;
    private readonly TimeSpan _period;
    private readonly InMemoryState _inMemoryState;
    private readonly RemoteSyncQueue _toRemoteQueue;
    private readonly LocalHandledChangesQueue _localHandledChangesQueue;
    private readonly InMemorySnapshotQueue _snapshotQueue;
    private readonly LocalSyncErrorsQueue _errorsQueue;
    private readonly ParserErrorQueue _parserErrorQueue;
    private readonly FactoryErrorQueue _factoryErrorQueue;

    public LocalSyncWorker(ILogger<LocalSyncWorker> logger,
        InMemoryState inMemoryState,
        RemoteSyncQueue toRemoteQueue,
        LocalHandledChangesQueue localHandledChangesQueue,
        InMemorySnapshotQueue snapshotQueue,
        LocalSyncErrorsQueue errorsQueue,
        ParserErrorQueue parserErrorQueue,
        FactoryErrorQueue factoryErrorQueue)
    {
        _logger = logger;
        _period = TimeSpan.FromSeconds(1);
        _inMemoryState = inMemoryState;
        _toRemoteQueue = toRemoteQueue;
        _localHandledChangesQueue = localHandledChangesQueue;
        _snapshotQueue = snapshotQueue;
        _errorsQueue = errorsQueue;
        _parserErrorQueue = parserErrorQueue;
        _factoryErrorQueue = factoryErrorQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.Run(async () =>
    {
        _logger.LogInformation("LocalSyncWorker Started");
        bool isFirstRun = true;
        using PeriodicTimer timer = new(_period);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (isFirstRun)
                {
                    if (await InitState(stoppingToken))
                    {
                        isFirstRun = false;
                        _logger.LogInformation("InitState done");
                    }
                    else
                    {
                        _logger.LogError("InitState problem. Will retry in a few moment");
                        continue;
                    }
                }

                await PipelineProcess(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException &&
                                      e is not TaskCanceledException)
            {
                _logger.LogError(e, "Exception catched");
            }
        }
    });

    private async Task<bool> InitState(CancellationToken stoppingToken)
    {
        try
        {
            using var db = new DAL.SqlServer.DB();

            //set as ignored 'I' all the changes of the db not locally sync
            var localIgnored = await db.ChangedRecordsQueues
                .Where(x => x.LocalSync == "F")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.LocalSync, p => "I")
                );
            _logger.LogInformation("InitState set {localIgnored} changes as Ignored", localIgnored);

            //set as ignored 'I' all the changes of the db not remotely sync
            var remoteIgnored = await db.ChangedRecordsQueues
                .Where(x => x.RemoteSync == "F")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.RemoteSync, p => "I")
                );
            _logger.LogInformation("InitState set {remoteIgnored} changes as Ignored", remoteIgnored);

            var aree = await db.Arees
                .ProjectToType<Area>()
                .ToListAsync(cancellationToken: stoppingToken);
            _logger.LogInformation("InitState load {areasCount} areas", aree.Count);

            var insediamenti = await db.Insediamentis
                .ProjectToType<Insediamento>()
                .ToListAsync(cancellationToken: stoppingToken);
            _logger.LogInformation("InitState load {insediamentisCount} insediamentis", insediamenti.Count);

            //init local in memory state
            if (_inMemoryState.Init(insediamenti, aree) == false)
            {
                _logger.LogError("InitState failed");
                return false;
            }

            //add to snapshot queue to init the remote memory state
            _snapshotQueue.Add(_inMemoryState.GetSnapshot());

            _logger.LogInformation("InitState success");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "InitState Exception");
            return false;
        }
    }

    private async Task PipelineProcess(CancellationToken stoppingToken)
    {
        var list = await LoadUnprocessedChangedRecords(stoppingToken);
        foreach (var changeEvent in list)
        {
            _logger.LogInformation("Start processing a record from db with id {id}", changeEvent.Id);

            //start parsing the changed record value
            var changedRecordValue = changeEvent.Parse();
            if (changedRecordValue.IsOk == false)
            {
                if (changedRecordValue.HasException)
                {
                    _logger.LogError(
                        changedRecordValue.Exception,
                        "The parser throw an exception to handle the record from db {changeEvent}", changeEvent);
                }
                else
                {
                    _logger.LogError("The parser fail to handle the record from db {changeEvent}", changeEvent);
                }

                _parserErrorQueue.Enqueue(new ParserError(changeEvent, changedRecordValue.Exception));
                continue;
            }

            var changeInfo = changeEvent.Adapt<ChangedRecordInfo>();
            var createdObjectResult = changeInfo.Create(changedRecordValue.Result!);
            if (createdObjectResult.IsOk == false)
            {
                if (createdObjectResult.HasException)
                {
                    _logger.LogError(
                        createdObjectResult.Exception,
                        "The factory throw an exception to handle the record from db {changeEvent}", changeEvent);
                }
                else
                {
                    _logger.LogError("The factory fail to handle the change {changeEvent}", changeEvent);
                }

                _factoryErrorQueue.Enqueue(new FactoryError(changeInfo, createdObjectResult.Result,
                    createdObjectResult.Exception));
                continue;
            }

            //here we work with the parsed change object
            IRecordChange changedRecord = createdObjectResult.Result;

            //update in memory state
            if (_inMemoryState.PushChange(changedRecord) == false)
            {
                _errorsQueue.Enqueue(changedRecord);
                _logger.LogWarning("The memory state manager fail to handle the record from db {changedRecord}",
                    changedRecord);
                continue;
            }

            //in memory state updated successfully
            _logger.LogInformation("Update local inMemoryState");

            //send change to cloud
            _toRemoteQueue.Add(changedRecord);

            //here we save on the db that the record has been handled (locally)
            _localHandledChangesQueue.Add(changeEvent.Id, stoppingToken);
        }
    }

    private async Task<List<ChangedRecordsQueue>> LoadUnprocessedChangedRecords(CancellationToken stoppingToken)
    {
        using var db = new DAL.SqlServer.DB();
        var list = await db.ChangedRecordsQueues
            .Where(x => x.LocalSync == "F")
            .OrderBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogDebug("Loaded {recordCount} records from db", list.Count);
        return list;
    }
}