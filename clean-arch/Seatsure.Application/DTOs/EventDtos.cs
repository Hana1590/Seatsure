using Seatsure.Domain;

namespace Seatsure.Application.DTOs;

public record EventDto(Guid Id, Guid OrganizerId, string Title, string Description, string Venue, DateTime StartsAtUtc, EventStatus Status);
public record CreateEventDto(string Title, string Description, string Venue, DateTime StartsAtUtc);
public record EventDetailsDto(Guid Id, Guid OrganizerId, string Title, string Description, string Venue, DateTime StartsAtUtc, EventStatus Status, IEnumerable<TicketTypes.TicketTypeDto> Tickets);
