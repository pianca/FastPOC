using System.Collections.Concurrent;
using FEPOC.Models.DTO;

namespace FEPOC.DataSource.Utility;

public static class ChangeRecordFactory
{
    public static CreatedObjectResult Create(this ChangedRecordInfo info, object record)
    {
        try
        {
            switch (info.RecordTable)
            {
                case Area.DdType:
                    return new CreatedObjectResult(new ChangedArea(info, record as Area));
                case Insediamento.DdType:
                    return new CreatedObjectResult(new ChangedInsediamento(info,record as Insediamento));
                default:
                    return new CreatedObjectResult();
            }
        }
        catch (Exception e)
        {
            return new CreatedObjectResult(e);
        }
    }
}

public record CreatedObjectResult : OptionalObjectResult<IRecordChange>
{
    public CreatedObjectResult()
    {
    }

    public CreatedObjectResult(IRecordChange result) : base(result)
    {
    }

    public CreatedObjectResult(Exception exception) : base(exception)
    {
    }
}

public record FactoryError(ChangedRecordInfo ChangedRecordInfo, IRecordChange ChangedRecord, Exception Exception);

public class FactoryErrorQueue : ConcurrentQueue<FactoryError>
{
}