using Microsoft.AspNetCore.SignalR;
using Seatsure.Application.Notifications;
using Seatsure.Hubs;

namespace Seatsure.Notifications;

public sealed class SignalRAvailabilityNotification : IAvailabilityNotification
{
    private readonly IHubContext<AvailabilityHub> _hub;
    public SignalRAvailabilityNotification(IHubContext<AvailabilityHub> hub) => _hub = hub;
    public Task AvailabilityChangedAsync(Guid eventId, Guid ticketTypeId, int availableQuantity) =>
        _hub.Clients.Group($"event-{eventId}").SendAsync("AvailabilityChanged", ticketTypeId, availableQuantity);
}
