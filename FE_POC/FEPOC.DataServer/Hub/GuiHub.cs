using FEPOC.DataServer.Services;

namespace FEPOC.DataServer.Hub;

using Microsoft.AspNetCore.SignalR;

public class GuiHub : Hub
{
    private readonly ILogger<GuiHub> _logger;
    private readonly GuiDataStream _stream;

    public GuiHub(ILogger<GuiHub> logger, GuiDataStream stream)
    {
        _logger = logger;
        _stream = stream;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("New GUI Client {connectionId}", Context.ConnectionId);
        _stream.AddClient(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            _logger.LogInformation("Disconnected GUI Client {connectionId}", Context.ConnectionId);
        else
            _logger.LogError(exception, "Disconnected GUI Client {connectionId} with exception", Context.ConnectionId);
        _stream.RemoveClient(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}