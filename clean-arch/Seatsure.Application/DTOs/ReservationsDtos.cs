namespace Seatsure.Application.DTOs.Reservation;
using Seatsure.Domain;

public record CreateReservationDto(int Quantity);
public record ReservationDto(Guid Id, Guid TicketTypeId, int Quantity, ReservationStatus Status,
    DateTime HoldExpiresAtUtc, DateTime CreatedAtUtc, DateTime? ConfirmedAtUtc);
