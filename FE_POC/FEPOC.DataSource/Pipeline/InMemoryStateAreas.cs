using FEPOC.DataSource.DTO;
using Mapster;

namespace FEPOC.DataSource.Pipeline;

public partial class InMemoryState
{
    public bool PushChange(Area a, ChangedRecordInfo change)
    {
        switch (change.ChangeType)
        {
            case "I":
                return Insert(a,change);
            case "U":
                return Update(a,change);
            case "D":
                return Delete(a,change);
            default:
                throw new NotSupportedException($"Change type = {change.ChangeType}");
        }
    }

    private bool Insert(Area a, ChangedRecordInfo change)
    {
        if (CannotIns(a))
        {
            _logger.LogError("Ins failed for {area} of change {change}", a, change);
            return false;
        }

        _aree[a.Id] = a;
        var insedKey = a.IdInsediamento.Adapt<InsediamentoId>();
        if (_insediamentiWithAreas.ContainsKey(insedKey))
        {
            _insediamentiWithAreas[insedKey].Add(a.Id, a.Adapt<AreaId>());
        }
        else
        {
            _logger.LogWarning("Insediamento {insediamentoId} not found", insedKey.Id);
        }
        return true;
    }

    private bool Delete(Area a, ChangedRecordInfo change)
    {
        if (CannotDel(a))
        {
            _logger.LogError("Del failed for {area} of change {change}", a, change);
            return false;
        }

        _aree.Remove(a.Id);
        var insedKey = a.IdInsediamento.Adapt<InsediamentoId>();
        if (_insediamentiWithAreas.ContainsKey(insedKey))
        {
            _insediamentiWithAreas[insedKey].Remove(a.Id);
        }
        else
        {
            _logger.LogWarning("Insediamento {insediamentoId} not found", insedKey.Id);
        }
        return true;
    }
    
    private bool Update(Area newA, ChangedRecordInfo change)
    {
        if (CannotUpd(newA))
        {
            _logger.LogError("Upd failed for {area} of change {change}", newA, change);
            return false;
        }

        var oldA = _aree[newA.Id];
        _aree[newA.Id] = newA;
        if (oldA.IdInsediamento != newA.IdInsediamento)
        {
            var oldInsedKey = oldA.IdInsediamento.Adapt<InsediamentoId>();
            if (_insediamentiWithAreas.ContainsKey(oldInsedKey))
            {
                _insediamentiWithAreas[oldInsedKey].Remove(oldA.Id);
            }
            else
            {
                _logger.LogWarning("Old Insediamento {insediamentoId} not found", oldInsedKey.Id);
            }

            var insedKey = newA.IdInsediamento.Adapt<InsediamentoId>();
            if (_insediamentiWithAreas.ContainsKey(insedKey))
            {
                _insediamentiWithAreas[insedKey].Add(newA.Id, newA.Adapt<AreaId>());
            }
            else
            {
                _logger.LogWarning("New Insediamento {insediamentoId} not found", insedKey.Id);
            }
        }

        return true;
    }

    private bool CannotDel(Area area) => _aree.ContainsKey(area.Id) == false;
    private bool CannotUpd(Area area) => _aree.ContainsKey(area.Id) == false;
    private bool CannotIns(Area area) => _aree.ContainsKey(area.Id);
}