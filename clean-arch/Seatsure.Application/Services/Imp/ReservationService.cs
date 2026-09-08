using Microsoft.EntityFrameworkCore;
using Seatsure.Application.DTOs.Reservation;
using Seatsure.Application.Exceptions;
using Seatsure.Application.Interfaces;
using Seatsure.Application.Notifications;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;

namespace Seatsure.Application.Services.Imp;

public sealed class ReservationService : IReservationService
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);
    private readonly IReservationRepository _reservations;
    private readonly ITicketTypeRepository _tickets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAvailabilityNotification _notifier;

    public ReservationService(IReservationRepository reservations, ITicketTypeRepository tickets, IUnitOfWork unitOfWork, IAvailabilityNotification notifier)
    { _reservations = reservations; _tickets = tickets; _unitOfWork = unitOfWork; _notifier = notifier; }

    public async Task<ReservationDto> CreateHoldAsync(Guid callerId, Guid ticketTypeId, CreateReservationDto request)
    {
        if (request.Quantity < 1) throw new ValidationException("Quantity must be at least 1.");
        var ticket = await _tickets.GetTicketTypeByIdAsync(ticketTypeId) ?? throw new NotFoundException("Ticket type not found.");
        if (request.Quantity > ticket.AvailableQuantity) throw new ConflictException("Insufficient ticket inventory.");

        ticket.AvailableQuantity -= request.Quantity;
        var reservation = new Reservation { TicketTypeId = ticket.Id, UserId = callerId, Quantity = request.Quantity,
            Status = ReservationStatus.Pending, HoldExpiresAtUtc = DateTime.UtcNow.Add(HoldDuration), CreatedAtUtc = DateTime.UtcNow };
        await _reservations.AddAsync(reservation);
        try { await _unitOfWork.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { throw new ConflictException("The ticket inventory changed. Please retry the reservation."); }
        await _notifier.AvailabilityChangedAsync(ticket.EventId, ticket.Id, ticket.AvailableQuantity);
        return ToDto(reservation);
    }

    public async Task<ReservationDto> ConfirmAsync(Guid callerId, Guid reservationId)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId) ?? throw new NotFoundException("Reservation not found.");
        EnsureOwner(reservation, callerId);
        if (reservation.Status != ReservationStatus.Pending) throw new ConflictException("Only pending reservations can be confirmed.");
        if (reservation.HoldExpiresAtUtc <= DateTime.UtcNow) throw new ConflictException("Reservation hold has expired.");
        reservation.Status = ReservationStatus.Confirmed;
        reservation.ConfirmedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        await _notifier.AvailabilityChangedAsync(reservation.TicketType.EventId, reservation.TicketTypeId, reservation.TicketType.AvailableQuantity);
        return ToDto(reservation);
    }

    public async Task<ReservationDto> CancelAsync(Guid callerId, Guid reservationId)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId) ?? throw new NotFoundException("Reservation not found.");
        EnsureOwner(reservation, callerId);
        if (reservation.Status != ReservationStatus.Pending) throw new ConflictException("Only pending reservations can be cancelled.");

        var ticket = reservation.TicketType;
        ticket.AvailableQuantity += reservation.Quantity;
        reservation.Status = ReservationStatus.Cancelled;
        try { await _unitOfWork.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { throw new ConflictException("The ticket inventory changed. Please retry the cancellation."); }
        await _notifier.AvailabilityChangedAsync(ticket.EventId, ticket.Id, ticket.AvailableQuantity);
        return ToDto(reservation);
    }

    public async Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(Guid callerId)
    {
        var reservations = await _reservations.GetByUserIdAsync(callerId);
        return reservations.Select(ToDto).ToList();
    }

    public async Task<int> ExpirePendingHoldsAsync(CancellationToken cancellationToken = default)
    {
        var expired = (await _reservations.GetExpiredHoldAsync()).ToList();
        var count = 0;
        foreach (var reservation in expired)
        {
            if (reservation.Status != ReservationStatus.Pending) continue;
            reservation.Status = ReservationStatus.Expired;
            reservation.TicketType.AvailableQuantity += reservation.Quantity;
            count++;
        }
        if (count > 0) await _unitOfWork.SaveChangesAsync(cancellationToken);
        foreach (var reservation in expired.Where(r => r.Status == ReservationStatus.Expired))
            await _notifier.AvailabilityChangedAsync(reservation.TicketType.EventId, reservation.TicketTypeId, reservation.TicketType.AvailableQuantity);
        return count;
    }

    private static void EnsureOwner(Reservation r, Guid callerId)
    { if (r.UserId != callerId) throw new ForbiddenException("You do not own this reservation."); }

    private static ReservationDto ToDto(Reservation r) => new(r.Id, r.TicketTypeId, r.Quantity, r.Status, r.HoldExpiresAtUtc, r.CreatedAtUtc, r.ConfirmedAtUtc);
}
