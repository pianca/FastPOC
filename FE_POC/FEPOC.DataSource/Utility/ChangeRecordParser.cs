using System.Collections.Concurrent;
using System.Text.Json;
using FEPOC.DataSource.DAL.Models;
using FEPOC.Models.DTO;

namespace FEPOC.DataSource.Utility;

public static class ChangeRecordParser
{
    private static readonly JsonSerializerOptions _settings = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static ParsedObjectResult Parse(this ChangedRecordsQueue changed)
    {
        try
        {
            switch (changed.RecordTable)
            {
                case Area.DdType:
                    return new ParsedObjectResult(
                        JsonSerializer.Deserialize<Area[]>(changed.RecordValue, _settings)
                            ?.FirstOrDefault()
                    );

                case Insediamento.DdType:
                    return new ParsedObjectResult(
                        JsonSerializer.Deserialize<Insediamento[]>(changed.RecordValue, _settings)
                            ?.FirstOrDefault()
                    );
                default:
                    return new ParsedObjectResult();
            }
        }
        catch (Exception e)
        {
            return new ParsedObjectResult(e);
        }
    }
}

public record ParsedObjectResult : OptionalObjectResult<object>
{
    public ParsedObjectResult() : base()
    {
    }

    public ParsedObjectResult(object result) : base(result)
    {
    }

    public ParsedObjectResult(Exception exception) : base(exception)
    {
    }
}

public record ParserError(ChangedRecordsQueue Change, Exception? Exception)
{
    public bool HasException => Exception != null;
}

public class ParserErrorQueue : ConcurrentQueue<ParserError>
{
}