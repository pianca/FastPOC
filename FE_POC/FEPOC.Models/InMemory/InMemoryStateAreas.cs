using System;
using FEPOC.Models.DTO;
using Microsoft.Extensions.Logging;


namespace FEPOC.Models.InMemory;

public partial class InMemoryState
{
    private bool PushChange(Area a, ChangedRecordInfo change)
    {
        bool ok = false;
        switch (change.ChangeType)
        {
            case "I":
                ok = Insert(a,change);
                break;
            case "U":
                ok = Update(a,change);
                break;
            case "D":
                ok = Delete(a,change);
                break;
            default:
                throw new NotSupportedException($"PushChange: Change type = {change.ChangeType}");
        }

        if (ok)
        {
            Version = new Version(change.Id, change.Timestamp, DateTimeOffset.Now);
        }

        return ok;
    }

    private bool Insert(Area a, ChangedRecordInfo change)
    {
        if (CannotIns(a))
        {
            _logger.LogError("Ins failed for {area} of change {change}", a, change);
            return false;
        }

        _aree[a.Id] = a;
        
        if (_insediamentiWithAreas.ContainsKey(a.IdInsediamento))
        {
            _insediamentiWithAreas[a.IdInsediamento].Add(a.Id);
        }
        else
        {
            _logger.LogWarning("Insediamento {insediamentoId} not found", a.IdInsediamento);
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
        
        if (_insediamentiWithAreas.ContainsKey(a.IdInsediamento))
        {
            _insediamentiWithAreas[a.IdInsediamento].Remove(a.Id);
        }
        else
        {
            _logger.LogWarning("Insediamento {insediamentoId} not found", a.IdInsediamento);
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
            if (_insediamentiWithAreas.ContainsKey(oldA.IdInsediamento))
            {
                _insediamentiWithAreas[oldA.IdInsediamento].Remove(oldA.Id);
            }
            else
            {
                _logger.LogWarning("Old Insediamento {insediamentoId} not found", oldA.IdInsediamento);
            }

            if (_insediamentiWithAreas.ContainsKey(newA.IdInsediamento))
            {
                _insediamentiWithAreas[newA.IdInsediamento].Add(newA.Id);
            }
            else
            {
                _logger.LogWarning("New Insediamento {insediamentoId} not found", newA.IdInsediamento);
            }
        }

        return true;
    }

    private bool CannotDel(Area area) => _aree.ContainsKey(area.Id) == false;
    private bool CannotUpd(Area area) => _aree.ContainsKey(area.Id) == false;
    private bool CannotIns(Area area) => _aree.ContainsKey(area.Id);
}