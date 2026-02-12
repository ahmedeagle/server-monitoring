using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ServerMonitoring.API.Hubs;

/// <summary>
/// SignalR Hub for real-time monitoring updates
/// Provides real-time metrics, alerts, and notifications to connected clients
/// </summary>
[Authorize]
public class MonitoringHub : Hub
{
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(ILogger<MonitoringHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("Client connected. UserId: {UserId}, ConnectionId: {ConnectionId}", 
            userId, connectionId);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error. UserId: {UserId}, ConnectionId: {ConnectionId}", 
                userId, connectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected. UserId: {UserId}, ConnectionId: {ConnectionId}", 
                userId, connectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to server metrics updates
    /// </summary>
    /// <param name="serverId">Server ID to subscribe to</param>
    public async Task SubscribeToServer(int serverId)
    {
        var groupName = $"server_{serverId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("Client {ConnectionId} subscribed to server {ServerId}", 
            Context.ConnectionId, serverId);
    }

    /// <summary>
    /// Unsubscribe from server metrics updates
    /// </summary>
    /// <param name="serverId">Server ID to unsubscribe from</param>
    public async Task UnsubscribeFromServer(int serverId)
    {
        var groupName = $"server_{serverId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("Client {ConnectionId} unsubscribed from server {ServerId}", 
            Context.ConnectionId, serverId);
    }

    /// <summary>
    /// Subscribe to all alerts
    /// </summary>
    public async Task SubscribeToAlerts()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");
        
        _logger.LogInformation("Client {ConnectionId} subscribed to alerts", 
            Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from all alerts
    /// </summary>
    public async Task UnsubscribeFromAlerts()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alerts");
        
        _logger.LogInformation("Client {ConnectionId} unsubscribed from alerts", 
            Context.ConnectionId);
    }

    /// <summary>
    /// Send heartbeat to keep connection alive
    /// </summary>
    public Task Heartbeat()
    {
        return Task.CompletedTask;
    }
}
