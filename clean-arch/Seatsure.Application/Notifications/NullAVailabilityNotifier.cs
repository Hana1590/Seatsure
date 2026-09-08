namespace Seatsure.Application.Notifications;

public sealed class NullAvailabilityNotifier : IAvailabilityNotification
{
    public Task AvailabilityChangedAsync(Guid eventId, Guid ticketTypeId, int availableQuantity) => Task.CompletedTask;
}
