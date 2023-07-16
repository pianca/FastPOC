namespace FEPOC.DataSource.DTO;

public class ChangedRecordInfo
{
    public long Id { get; set; }

    public DateTime Timestamp { get; set; }

    public string ChangeType { get; set; } = null!;

    public string RecordTable { get; set; } = null!;

    // public string RecordValue { get; set; } = null!;
    //
    // public string LocalSync { get; set; } = null!;
    //
    // public string RemoteSync { get; set; } = null!;
}
