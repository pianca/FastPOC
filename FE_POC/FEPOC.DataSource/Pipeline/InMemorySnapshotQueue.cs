using System.Collections.Concurrent;
using FEPOC.Models.InMemory;

namespace FEPOC.DataSource.Pipeline;

public class InMemorySnapshotQueue : BlockingCollection<InMemoryStateSnapshot>
{
    
}