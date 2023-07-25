using FEPOC.Models.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FEPOC.Models.InMemory;

public record Version(long Id, DateTime DbTimestamp, DateTimeOffset ElabTimestamp);

public partial class InMemoryState
{
    private readonly SortedList<int, Insediamento>
        _insediamenti = new SortedList<int, Insediamento>(20);

    private readonly SortedList<int, Area>
        _aree = new SortedList<int, Area>(100);

    private readonly SortedDictionary<int, SortedSet<int>>
        _insediamentiWithAreas = new SortedDictionary<int, SortedSet<int>>();

    private readonly ILogger<InMemoryState> _logger;

    public Version? Version { get; private set; }

    public InMemoryState(ILogger<InMemoryState> logger)
    {
        _logger = logger;
    }

    public bool Init(List<Insediamento> insediamentis, List<Area> areas)
    {
        try
        {
            _insediamenti.Clear();
            foreach (var i in insediamentis)
            {
                _insediamenti.Add(i.Id, i);
            }

            _aree.Clear();
            foreach (var a in areas)
            {
                _aree.Add(a.Id, a);
            }

            _insediamentiWithAreas.Clear();
            foreach (var insedId in _insediamenti.Keys)
            {
                var areasOfInsed = _aree.Values
                    .Where(x => x.IdInsediamento == insedId)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .ToArray();
                _insediamentiWithAreas[insedId] = new SortedSet<int>(areasOfInsed);
            }

            Version = new Version(0, DateTime.Now, DateTimeOffset.Now);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Init: Catched exception with insediamentis {insediamentis} and areas {areas}",
                insediamentis,
                areas);
            return false;
        }
    }

    public InMemoryStateSnapshot GetSnapshot()
    {
        return new InMemoryStateSnapshot
        {
            Insediamenti = new SortedList<int, Insediamento>(_insediamenti),
            Aree = new SortedList<int, Area>(_aree),
            InsediamentiWithAree = new SortedDictionary<int, SortedSet<int>>(_insediamentiWithAreas),
            Version = Version
        };
    }
    
    public void PrintState()
    {
        _logger.LogInformation(JsonConvert.SerializeObject(_insediamenti, Formatting.Indented));
        _logger.LogInformation(JsonConvert.SerializeObject(_aree, Formatting.Indented));
        _logger.LogInformation(JsonConvert.SerializeObject(_insediamentiWithAreas, Formatting.Indented));
    }
    
    public bool PushChange(IRecordChange change)
    {
        if (change is ChangedArea)
            return PushChange(change as ChangedArea);
        if (change is ChangedInsediamento)
            return PushChange(change as ChangedInsediamento);

        throw new NotSupportedException($"The record change type {change.GetType().Name} is not supported");
    }
    public bool PushChange(ChangedArea change)=>this.PushChange(change.Record!, change.Info);
    public bool PushChange(ChangedInsediamento change)=>this.PushChange(change.Record!, change.Info);
}