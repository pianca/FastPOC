using FastEndpoints;
using FEPOC.Contracts;
using FEPOC.Models.InMemory;

namespace FEPOC.DataServer.Handlers;

public sealed class SyncChangeInsediamentoHandler : ICommandHandler<SyncChangeInsediamentoCommand, SyncChangeResult>
{
    private readonly ILogger<SyncChangeInsediamentoHandler> _logger;
    private readonly InMemoryState _inMemory;

    public SyncChangeInsediamentoHandler(ILogger<SyncChangeInsediamentoHandler> logger, InMemoryState inMemory)
    {
        _logger = logger;
        _inMemory = inMemory;
    }

    public Task<SyncChangeResult> ExecuteAsync(SyncChangeInsediamentoCommand command, CancellationToken ct = default)
    {
        try
        {
            if (_inMemory.PushChange(command.RecordChange))
            {
                _logger.LogInformation("SyncChangeInsediamentoHandler ok");
                return Task.FromResult(new SyncChangeResult()
                {
                    Success = true,
                    Message = "OK"
                });
            }
            else
            {               
                _logger.LogWarning("SyncChangeInsediamentoHandler fail");
                return Task.FromResult(new SyncChangeResult()
                {
                    Success = false,
                    Message = "FAIL"
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SyncChangeInsediamentoHandler exception");            
            return Task.FromResult(new SyncChangeResult()
            {
                Success = false,
                Message = "EXCEPTION: " + e.Message
            });
        }
    }
}