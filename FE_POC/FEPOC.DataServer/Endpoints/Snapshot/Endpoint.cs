using FastEndpoints;
using FEPOC.Models.InMemory;

namespace FEPOC.DataServer.Endpoints.Snapshot;

public class GetSnapshot : EndpointWithoutRequest<InMemoryStateSnapshot>
{
    private readonly InMemoryState _state;

    public GetSnapshot(InMemoryState state)
    {
        _state = state;
    }

    public override void Configure()
    {
        Get("state");
        AllowAnonymous();
    }

    public override Task<InMemoryStateSnapshot> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(_state.GetSnapshot());
    }
    // public override async Task HandleAsync(CancellationToken c)
    // {
    //     var list = new List<WeatherForecast>();
    //     for (int i = 1; i <= r.AmountToGet; i++)
    //     {
    //         list.Add(new()
    //         {
    //             Date = DateTime.UtcNow.AddDays(i),
    //             Summary = $"i am {i}",
    //             TemperatureC = i + 34
    //         });
    //     }
    //     await SendAsync(list.ToArray());
    // }
}