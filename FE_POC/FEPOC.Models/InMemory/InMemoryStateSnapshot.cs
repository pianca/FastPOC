
using System.Collections.Generic;
using FEPOC.Models.DTO;

namespace FEPOC.Models.InMemory;

public record InMemoryStateSnapshot
{
    public SortedList<int, Insediamento> Insediamenti { get; init; }
    public SortedList<int, Area> Aree { get; init; }
    public SortedDictionary<int, SortedSet<int>> InsediamentiWithAree { get; init; }
    public Version Version { get; init; }
}