using FEPOC.DataSource.DTO;
using Mapster;

namespace FEPOC.DataSource.Pipeline;

public partial class InMemoryState
{
    public bool PushChange(Insediamento i, ChangedRecordInfo change)
    {
        switch (change.ChangeType)
        {
            case "I":
                return Insert(i,change);
            case "U":
                return Update(i,change);
            case "D":
                return Delete(i,change);
            default:
                throw new NotSupportedException($"Change type = {change.ChangeType}");
        }
    }

    private bool Insert(Insediamento i, ChangedRecordInfo change)
    {
        if (CannotIns(i))
        {
            _logger.LogError("Ins failed for {area} of change {change}", i, change);
            return false;
        }

        _insediamenti[i.Id] = i;
        var insedKey = i.Id.Adapt<InsediamentoId>();
        _insediamentiWithAreas[insedKey].Add(i.Id, i.Adapt<AreaId>());
        return true;
    }

    private bool Delete(Insediamento i, ChangedRecordInfo change)
    {
        if (CannotDel(i))
        {
            _logger.LogError("Del failed for {area} of change {change}", i, change);
            return false;
        }

        _insediamenti.Remove(i.Id);
        var insedKey = i.Id.Adapt<InsediamentoId>();
        _insediamentiWithAreas[insedKey].Remove(i.Id);
        return true;
    }
    
    private bool Update(Insediamento newI, ChangedRecordInfo change)
    {
        if (CannotUpd(newI))
        {
            _logger.LogError("Upd failed for {area} of change {change}", newI, change);
            return false;
        }

        var oldI = _insediamenti[newI.Id];
        _insediamenti[newI.Id] = newI;
        return true;
    }

    private bool CannotDel(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id) == false;
    private bool CannotUpd(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id) == false;
    private bool CannotIns(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id);
}