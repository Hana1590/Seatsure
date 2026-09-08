using Microsoft.AspNetCore.SignalR;

namespace Seatsure.Hubs;

public sealed class AvailabilityHub : Hub
{
    public Task JoinEvent(Guid eventId) => Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
    public Task LeaveEvent(Guid eventId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
}
