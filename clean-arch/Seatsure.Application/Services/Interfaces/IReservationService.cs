using Seatsure.Application.DTOs.Reservation;

namespace Seatsure.Application.Services.Interfaces;

public interface IReservationService
{
    Task<ReservationDto> CreateHoldAsync(Guid callerId, Guid ticketTypeId, CreateReservationDto request);
    Task<ReservationDto> ConfirmAsync(Guid callerId, Guid reservationId);
    Task<ReservationDto> CancelAsync(Guid callerId, Guid reservationId);
    Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(Guid callerId);
    Task<int> ExpirePendingHoldsAsync(CancellationToken cancellationToken = default);
}
