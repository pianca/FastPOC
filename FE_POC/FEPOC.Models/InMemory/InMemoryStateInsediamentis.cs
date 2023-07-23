using System;
using System.Collections.Generic;
using FEPOC.Models.DTO;
using Microsoft.Extensions.Logging;

namespace FEPOC.Models.InMemory;

public partial class InMemoryState
{
    private bool PushChange(Insediamento i, ChangedRecordInfo change)
    {
        bool ok = false;
        switch (change.ChangeType)
        {
            case "I":
                ok = Insert(i,change);  break;
            case "U":
                ok = Update(i,change);  break;
            case "D":
                ok = Delete(i,change);  break;
            default:
                throw new NotSupportedException($"PushChange: Change type = {change.ChangeType}");
        }
        
        if (ok)
        {
            Version = new Version(change.Id, change.Timestamp, DateTimeOffset.Now);
        }

        return ok;
    }

    private bool Insert(Insediamento i, ChangedRecordInfo change)
    {
        if (CannotIns(i))
        {
            _logger.LogError("Ins failed for {area} of change {change}", i, change);
            return false;
        }

        _insediamenti[i.Id] = i;
        _insediamentiWithAreas[i.Id] = new SortedSet<int>();
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
        _insediamentiWithAreas.Remove(i.Id);
        return true;
    }
    
    private bool Update(Insediamento newI, ChangedRecordInfo change)
    {
        if (CannotUpd(newI))
        {
            _logger.LogError("Upd failed for {area} of change {change}", newI, change);
            return false;
        }

        _insediamenti[newI.Id] = newI;
        return true;
    }

    private bool CannotDel(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id) == false;
    private bool CannotUpd(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id) == false;
    private bool CannotIns(Insediamento insediamenti) => _insediamenti.ContainsKey(insediamenti.Id);
}