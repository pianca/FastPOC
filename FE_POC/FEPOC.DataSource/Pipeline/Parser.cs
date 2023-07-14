using System.Text.Json;
using FEPOC.DataSource.DAL.Models;

namespace FEPOC.DataSource.Pipeline;

public static class Parser
{
    private static readonly JsonSerializerOptions _settings = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static object Parse(this ChangedRecordsQueue changed)
    {
        switch (changed.RecordTable)
        {
            case "AREE":
                return JsonSerializer.Deserialize<Aree[]>(changed.RecordValue, _settings)[0];
            case "INSEDIAMENTI":
                return JsonSerializer.Deserialize<Insediamenti[]>(changed.RecordValue, _settings)[0];
            default:
                throw new NotSupportedException();
        }
    }
}