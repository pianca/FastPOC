using System.Collections.Concurrent;
using FEPOC.Models.DTO;

namespace FEPOC.DataSource.Pipeline.Remote;

public class RemoteSyncQueue : BlockingCollection<IRecordChange>
{
    
}