using FEPOC.DataSource.DAL.Models;
using FEPOC.DataSource.DTO;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.Pipeline;

public class PipelineWorker : BackgroundService
{
    private readonly ILogger<PipelineWorker> _logger;
    private readonly TimeSpan _period;
    private readonly ToCloudQueue<ChangedRecordsQueue> _queue;
    private readonly InMemoryState _inMemoryState;
    private readonly LocalHandledChangesQueue _localHandledChangesQueue;

    public PipelineWorker(ILogger<PipelineWorker> logger, ToCloudQueue<ChangedRecordsQueue> queue, InMemoryState inMemoryState, LocalHandledChangesQueue localHandledChangesQueue)
    {
        _logger = logger;
        _period = TimeSpan.FromSeconds(13);
        _queue = queue;
        _inMemoryState = inMemoryState;
        _localHandledChangesQueue = localHandledChangesQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool isFirstRun = true;

        using PeriodicTimer timer = new(_period);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (isFirstRun)
                {
                    if (await ComputeStartingPoint(stoppingToken))
                    {
                        isFirstRun = false;
                        _logger.LogInformation("ComputeStartingPoint done");
                    }
                    else
                    {
                        _logger.LogError("ComputeStartingPoint problem. Will retry in a few moment");
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
    }

    private async Task<bool> ComputeStartingPoint(CancellationToken stoppingToken)
    {
        try
        {
            using var db = new DAL.SqlServer.DB();
            var aree = await db.Arees
                .ProjectToType<Area>()
                .ToListAsync(cancellationToken: stoppingToken);
            var insediamenti = await db.Insediamentis
                .ProjectToType<Insediamento>()
                .ToListAsync(cancellationToken: stoppingToken);
            _inMemoryState.Init(insediamenti, aree);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception catched");
            return false;
        }
    }

    private async Task PipelineProcess(CancellationToken stoppingToken)
    {
        var list = await LoadUnprocessedChangedRecords(stoppingToken);
        foreach (var change in list)
        {
            _logger.LogInformation("Start processing a record from db with id {id}", change.Id);
            
            //start parsing the changed record value
            var recordValue = change.Parse();
            if (recordValue.IsOk == false || recordValue.Result == null)
            {
                _logger.LogWarning("The parser fail to handle the record from db {record}", change);
                continue;
            }
            
            //update in memory state
            var changeInfo = change.Adapt<ChangedRecordInfo>();
            if (PushChange(changeInfo, recordValue) == false)
            {
                _logger.LogWarning("The memory state manager fail to handle the record from db {record}", change);
                continue;
            }

            _localHandledChangesQueue.Add(changeInfo.Id, stoppingToken);
            
            // TODO: fix 
            // await _queue.Enqueue(change, stoppingToken);
        }
    }

    private bool PushChange(ChangedRecordInfo change, ParsedObjectResult recordValue)
    {
        switch (recordValue.Type)
        {       
            case Area.DdType:
                return _inMemoryState.PushChange((Area)recordValue.Result, change);
            case Insediamento.DdType:
                return _inMemoryState.PushChange((Insediamento)recordValue.Result, change);
            default:
                _logger.LogWarning("The type {type} of the record is not supported", recordValue.Type);
                return false;
        }
    }

    private async Task<List<ChangedRecordsQueue>> LoadUnprocessedChangedRecords(CancellationToken stoppingToken)
    {
        using var db = new DAL.SqlServer.DB();
        var list = await db.ChangedRecordsQueues
            .Where(x => x.LocalSync == "F" && x.RemoteSync == "F")
            .OrderBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation("Loaded {recordCount} records from db", list.Count);
        return list;
    }
}