using FastEndpoints;
using FEPOC.Contracts;
using FEPOC.Models.InMemory;

namespace FEPOC.DataServer.Handlers;

public sealed class SyncInitHandler : ICommandHandler<SyncInitCommand, SyncInitResult>
{
    private readonly ILogger<SyncInitHandler> _logger;
    private readonly InMemoryState _inMemory;

    public SyncInitHandler(ILogger<SyncInitHandler> logger, InMemoryState inMemory)
    {
        _logger = logger;
        _inMemory = inMemory;
    }

    public Task<SyncInitResult> ExecuteAsync(SyncInitCommand command, CancellationToken ct = default)
    {
        try
        {
            if (_inMemory.Init(
                    command.Snapshot.Insediamenti.Values.ToList(),
                    command.Snapshot.Aree.Values.ToList())
               )
            {
                _logger.LogInformation("SyncInitHandler ok");
                return Task.FromResult(new SyncInitResult()
                {
                    Success = true,
                    Message = "OK"
                });
            }
            else
            {
                _logger.LogWarning("SyncInitHandler fail");
                return Task.FromResult(new SyncInitResult()
                {
                    Success = false,
                    Message = "FAIL"
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SyncInitHandler exception");
            return Task.FromResult(new SyncInitResult()
            {
                Success = false,
                Message = "EXCEPTION: " + e.Message
            });
        }
    }
}