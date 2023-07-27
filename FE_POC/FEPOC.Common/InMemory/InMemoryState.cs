using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FEPOC.Common.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FEPOC.Common.InMemory;

public record Version(long Id, DateTime DbTimestamp, DateTimeOffset ElabTimestamp);

public partial class InMemoryState
{
    private ReplaySubject<InMemoryStateSnapshotDTO> _snapshot;
    private ReplaySubject<IRecordChange> _changes;

    private readonly SortedList<int, Insediamento>
        _insediamenti = new SortedList<int, Insediamento>(20);

    private readonly SortedList<int, Area>
        _aree = new SortedList<int, Area>(100);

    private readonly SortedDictionary<int, SortedSet<int>>
        _insediamentiWithAreas = new SortedDictionary<int, SortedSet<int>>();

    private readonly ILogger<InMemoryState> _logger;

    private readonly Dictionary<Type, Func<IRecordChange, bool>> _pushFuncs;

    public IObservable<InMemoryStateSnapshotDTO> Snapshots { get; }
    public IObservable<IRecordChange> Changes { get; }
    public Version? Version { get; private set; }

    public InMemoryState(ILogger<InMemoryState> logger)
    {
        _logger = logger;
        _snapshot = new ReplaySubject<InMemoryStateSnapshotDTO>(1);
        _changes = new ReplaySubject<IRecordChange>(TimeSpan.FromMinutes(1));
        _pushFuncs = new Dictionary<Type, Func<IRecordChange, bool>>
        {
            { typeof(ChangedArea), x => InternalPushChange(x as ChangedArea) },
            { typeof(ChangedInsediamento), x => InternalPushChange(x as ChangedInsediamento) },
        };

        Snapshots = _snapshot.AsObservable();
        Changes = _changes.AsObservable();
    }

    public bool Init(InMemoryStateSnapshotDTO snapshot)
    {
        return Init(snapshot.Insediamenti, snapshot.Aree, 0, snapshot.Version);
    }

    public bool Init(List<Insediamento> insediamentis, List<Area> areas, long changeId, Version? version = null)
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

            Version = version ?? new Version(changeId, DateTime.Now, DateTimeOffset.Now);

            _stopwatch = Stopwatch.StartNew();
            _snapshot.OnNext(GetSnapshot());

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

    public InMemoryStateSnapshotDTO GetSnapshot()
    {
        return new InMemoryStateSnapshotDTO
        {
            Insediamenti = _insediamenti.Values.ToList(),
            Aree = _aree.Values.ToList(),
            InsediamentiWithAree = _insediamentiWithAreas.ToDictionary(
                x => x.Key,
                x => x.Value.ToList()),
            Version = Version
        };
    }

    public void PrintState()
    {
        _logger.LogInformation(JsonConvert.SerializeObject(_insediamenti, Formatting.Indented));
        _logger.LogInformation(JsonConvert.SerializeObject(_aree, Formatting.Indented));
        _logger.LogInformation(JsonConvert.SerializeObject(_insediamentiWithAreas, Formatting.Indented));
    }

    // public bool PushChange(IRecordChange change)
    // {
    //     if (change is ChangedArea)
    //         return PushChange(change as ChangedArea);
    //     if (change is ChangedInsediamento)
    //         return PushChange(change as ChangedInsediamento);
    //
    //     throw new NotSupportedException($"The record change type {change.GetType().Name} is not supported");
    // }

    private Stopwatch? _stopwatch = null;

    public bool PushChange(IRecordChange change)
    {
        if (_pushFuncs.TryGetValue(change.GetType(), out Func<IRecordChange, bool> func))
        {
            var result = func.Invoke(change);
            if (result)
            {
                _changes.OnNext(change);
                if (_stopwatch == null || _stopwatch.Elapsed > TimeSpan.FromSeconds(20))
                {
                    _stopwatch = Stopwatch.StartNew();
                    _snapshot.OnNext(GetSnapshot());
                }
            }

            return result;
        }

        // if (change is ChangedArea)
        //     return PushChange(change as ChangedArea);
        // if (change is ChangedInsediamento)
        //     return PushChange(change as ChangedInsediamento);

        //TODO: handle this in a more clever way
        throw new NotSupportedException($"The record change type {change.GetType().Name} is not supported");
    }

    private bool InternalPushChange(ChangedArea change) => this.PushChange(change.Record!, change.Info);
    private bool InternalPushChange(ChangedInsediamento change) => this.PushChange(change.Record!, change.Info);
}