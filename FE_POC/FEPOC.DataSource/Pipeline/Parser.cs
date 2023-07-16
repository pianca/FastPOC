using System.Text.Json;
using FEPOC.DataSource.DAL.Models;
using FEPOC.DataSource.DTO;

namespace FEPOC.DataSource.Pipeline;

public record ParsedObjectResult(string Type, bool IsOk, object? Result);

public static class Parser
{
    private static readonly JsonSerializerOptions _settings = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ParsedObjectResult Parse(this ChangedRecordsQueue changed)
    {
        switch (changed.RecordTable)
        {
            case Area.DdType:
                return new ParsedObjectResult(
                    Type:Area.DdType,
                    true,
                    Result: JsonSerializer.Deserialize<Area[]>(changed.RecordValue, _settings)
                        ?.FirstOrDefault()
                    );

            case Insediamento.DdType:
                return new ParsedObjectResult(
                    Type:Insediamento.DdType,
                    true,
                    Result: JsonSerializer.Deserialize<Insediamento[]>(changed.RecordValue, _settings)
                        ?.FirstOrDefault()
                    );
            default:
                return new ParsedObjectResult("", false, null);
        }
    }
}