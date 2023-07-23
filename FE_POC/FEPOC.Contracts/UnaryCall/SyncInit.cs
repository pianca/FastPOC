using FastEndpoints;
using FEPOC.Models.InMemory;

namespace FEPOC.Contracts;

public class SyncInitCommand : ICommand<SyncInitResult>
{
    public InMemoryStateSnapshot Snapshot { get; set; }
}

public class SyncInitResult : CommonResult
{
}