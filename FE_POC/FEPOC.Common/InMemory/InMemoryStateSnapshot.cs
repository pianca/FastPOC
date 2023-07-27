
using System.Collections.Generic;
using FEPOC.Common.DTO;

namespace FEPOC.Common.InMemory;

public record InMemoryStateSnapshot
{
    public SortedList<int, Insediamento> Insediamenti { get; init; }
    public SortedList<int, Area> Aree { get; init; }
    public SortedDictionary<int, SortedSet<int>> InsediamentiWithAree { get; init; }
    public Version Version { get; init; }
}

public record InMemoryStateSnapshotDTO
{
    public List<Insediamento> Insediamenti { get; init; }
    public List<Area> Aree { get; init; }
    public Dictionary<int, List<int>> InsediamentiWithAree { get; init; }
    public Version Version { get; init; }
}