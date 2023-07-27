using FastEndpoints;
using FEPOC.Common.InMemory;

namespace FEPOC.DataServer.Endpoints.Snapshot;

public class GetSnapshot : EndpointWithoutRequest<InMemoryStateSnapshotDTO>
{
    public InMemoryState InMemoryState { get; set; }
    public override void Configure()
    {
        Get("state/last");
        AllowAnonymous();
    }

    public override Task<InMemoryStateSnapshotDTO> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(InMemoryState.GetSnapshot());
    }
}