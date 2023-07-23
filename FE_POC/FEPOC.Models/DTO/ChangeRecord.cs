namespace FEPOC.Models.DTO;

public interface IRecordChange
{
    public ChangedRecordInfo Info { get; }
}

public record RecordChange<T> : IRecordChange
{
    public RecordChange(ChangedRecordInfo info, T record)
    {
        Info = info;
        Record = record;
    }
    public ChangedRecordInfo Info { get; }
    public T Record { get; }
}

public record ChangedArea : RecordChange<Area>
{
    public ChangedArea(ChangedRecordInfo info, Area record) : base(info, record)
    {
    }
}

public record ChangedInsediamento : RecordChange<Insediamento>
{
    public ChangedInsediamento(ChangedRecordInfo info, Insediamento record) : base(info, record)
    {
    }
}