using FastEndpoints;
using FEPOC.Models.DTO;

namespace FEPOC.Contracts;

public class SyncChangeAreaCommand : ICommand<SyncChangeResult>// ICommand<SyncChangeAreaResult>
{
    public ChangedArea RecordChange { get; set; }
}
public class SyncChangeInsediamentoCommand : ICommand<SyncChangeResult>// ICommand<SyncChangeInsediamentoResult>
{
    public ChangedInsediamento RecordChange { get; set; }
    
}

public class SyncChangeResult : CommonResult
{
    
}
// public class SyncChangeAreaResult : CommonResult
// {
// }public class SyncChangeInsediamentoResult : CommonResult
// {
// }