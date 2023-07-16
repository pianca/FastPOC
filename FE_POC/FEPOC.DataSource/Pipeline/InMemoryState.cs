using FEPOC.DataSource.DTO;
using Mapster;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FEPOC.DataSource.Pipeline;

public record InsediamentoId(int Id);

public class InsediamentoIdComparer : IComparer<InsediamentoId>
{
    public int Compare(InsediamentoId x, InsediamentoId y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return x.Id.CompareTo(y.Id);
    }
}

public record AreaId(int Id);

public record Version(long Id, DateTimeOffset Timestamp);

public partial class InMemoryState
{
    private SortedList<int, Insediamento>
        _insediamenti = new SortedList<int, Insediamento>(20);

    private SortedList<int, Area>
        _aree = new SortedList<int, Area>(100);

    private SortedDictionary<InsediamentoId, SortedList<int, AreaId>>
        _insediamentiWithAreas = new SortedDictionary<InsediamentoId, SortedList<int, AreaId>>(_comparer);

    private static readonly InsediamentoIdComparer _comparer = new InsediamentoIdComparer();
    private readonly ILogger<InMemoryState> _logger;

    public Version? Version { get; private set; } = null;

    public InMemoryState(ILogger<InMemoryState> logger)
    {
        _logger = logger;
    }

    public bool Init(List<Insediamento> insediamentis, List<Area> areas)
    {
        try
        {
            foreach (var i in insediamentis)
            {
                _insediamenti.Add(i.Id, i);
            }

            foreach (var a in areas)
            {
                _aree.Add(a.Id, a);
            }

            foreach (var insedId in _insediamenti.Keys)
            {
                var areasOfInsed = _aree.Values
                    .Where(x => x.IdInsediamento == insedId)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Adapt<AreaId>())
                    .ToDictionary(x => x.Id, x => x);
                _insediamentiWithAreas[insedId.Adapt<InsediamentoId>()] = new SortedList<int, AreaId>(areasOfInsed);
            }

            Console.WriteLine(JsonSerializer.Serialize(_insediamenti));
            Console.WriteLine(JsonSerializer.Serialize(_aree));
            Console.WriteLine(JsonSerializer.Serialize(_insediamentiWithAreas));
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Catched exception in State.Init");
            return false;
        }
    }
}