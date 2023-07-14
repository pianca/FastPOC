using FEPOC.Contracts;
using FastEndpoints;

namespace FEPOC.DataServer.Handlers;

public sealed class SayHelloHandler : ICommandHandler<SayHelloCommand>
{
    private readonly ILogger<SayHelloHandler> logger;

    public SayHelloHandler(ILogger<SayHelloHandler> logger)
    {
        this.logger = logger;
    }

    public Task ExecuteAsync(SayHelloCommand command, CancellationToken ct)
    {
        logger.LogInformation("Hello from {from}", command.From);
        return Task.CompletedTask;
    }
}