using System.Threading.Channels;
using FEPOC.DataSource.DAL.Models;

namespace FEPOC.DataSource.Pipeline;

public class ToCloudQueue<T> where T : ChangedRecordsQueue
{
    private readonly ILogger<ToCloudQueue<T>> _logger;
    private readonly Channel<T> _channel;

    public ToCloudQueue(ILogger<ToCloudQueue<T>> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(10000));
        // _queue = new ConcurrentQueue<object>();
    }

    public async ValueTask Enqueue(T item, CancellationToken cancellationToken)
    {
        await _channel.Writer.WaitToWriteAsync(cancellationToken);
        await _channel.Writer.WriteAsync(item, cancellationToken);
        _logger.LogInformation("Enqueued a record in the queue with id {id}", item.Id);
    }

    public async ValueTask<T> DequeueSingle(CancellationToken cancellationToken)
    {
        var item = await _channel.Reader.ReadAsync(cancellationToken);
        _logger.LogInformation("Dequeued a record from the queue with id {id}", item.Id);
        return item;
        // if (await _channel.Reader.WaitToReadAsync(cancellationToken))
        // {
        //     var item = await _channel.Reader.ReadAsync(cancellationToken);
        //     _logger.LogInformation("Dequeued a record from the queue");
        // }
        // _channel.Reader.
        // _logger.LogError("Strange problme Dequeued a record from queue");
        // return default(T);
    }
}