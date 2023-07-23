namespace FEPOC.DataSource.Utility;

public record OptionalObjectResult<T>
{
    public OptionalObjectResult()
    {
        Result = default;
        Exception = null;
    }

    public OptionalObjectResult(T result)
    {
        Result = result;
        Exception = null;
    }

    public OptionalObjectResult(Exception exception)
    {
        Result = default;
        Exception = exception;
    }

    public T? Result { get; private init; }
    public Exception? Exception { get; private init; }
    public bool IsOk => Result != null;
    public bool HasException => Exception != null;
}