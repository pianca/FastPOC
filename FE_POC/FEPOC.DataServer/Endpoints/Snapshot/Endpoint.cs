using FastEndpoints;
using FEPOC.Models.InMemory;

namespace FEPOC.DataServer.Endpoints.Snapshot;

public class GetSnapshot : EndpointWithoutRequest<InMemoryStateSnapshot>
{
    public InMemoryState InMemoryState { get; set; }
    public override void Configure()
    {
        Get("state/last");
        AllowAnonymous();
    }

    public override Task<InMemoryStateSnapshot> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(InMemoryState.GetSnapshot());
    }
}