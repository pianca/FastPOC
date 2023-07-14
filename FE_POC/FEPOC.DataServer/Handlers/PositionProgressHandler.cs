using FEPOC.Contracts;
using FastEndpoints;

namespace FEPOC.DataServer.Handlers;

public sealed class PositionProgressHandler : IClientStreamCommandHandler<CurrentPosition, ProgressReport>
{
    private readonly ILogger<PositionProgressHandler> _logger;

    public PositionProgressHandler(ILogger<PositionProgressHandler> logger)
    {
        _logger = logger;
    }

    public async Task<ProgressReport> ExecuteAsync(IAsyncEnumerable<CurrentPosition> stream, CancellationToken ct)
    {
        int currentNumber = 0;
        await foreach (var position in stream)
        {
            _logger.LogInformation("Current number: {pos}", position.Number);
            currentNumber = position.Number;
        }
        return new ProgressReport { LastNumber = currentNumber };
    }
}