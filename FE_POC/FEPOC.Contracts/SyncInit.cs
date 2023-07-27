using FastEndpoints;
using FEPOC.Common.InMemory;

namespace FEPOC.Contracts;

public class SyncInitCommand : ICommand<SyncInitResult>
{
    public InMemoryStateSnapshotDTO Snapshot { get; set; }
}

public class SyncInitResult : CommonResult
{
}