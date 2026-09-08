namespace Seatsure.Application.Notifications;

public interface IAvailabilityNotification
{
    Task AvailabilityChangedAsync(Guid eventId, Guid ticketTypeId, int availableQuantity);
}
