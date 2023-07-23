using FastEndpoints;
using FEPOC.Contracts;
using FEPOC.Models.InMemory;

namespace FEPOC.DataServer.Handlers;

public sealed class SyncChangeAreaHandler : ICommandHandler<SyncChangeAreaCommand, SyncChangeResult>//SyncChangeAreaResult>
{
    private readonly ILogger<SyncChangeAreaHandler> _logger;
    private readonly InMemoryState _inMemory;

    public SyncChangeAreaHandler(ILogger<SyncChangeAreaHandler> logger, InMemoryState inMemory)
    {
        _logger = logger;
        _inMemory = inMemory;
    }

    public Task<SyncChangeResult> ExecuteAsync(SyncChangeAreaCommand command, CancellationToken ct = default)
    {
        try
        {
            if (_inMemory.PushChange(command.RecordChange))
            {
                _logger.LogInformation("SyncChangeAreaHandler ok");
                return Task.FromResult(new SyncChangeResult()
                {
                    Success = true,
                    Message = "OK"
                });
            }
            else
            {               
                _logger.LogWarning("SyncChangeAreaHandler fail");
                return Task.FromResult(new SyncChangeResult()
                {
                    Success = false,
                    Message = "FAIL"
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SyncChangeAreaHandler exception");            
            return Task.FromResult(new SyncChangeResult()
            {
                Success = false,
                Message = "EXCEPTION: " + e.Message
            });
        }
    }
}