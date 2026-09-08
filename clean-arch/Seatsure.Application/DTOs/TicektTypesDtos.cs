namespace Seatsure.Application.DTOs.TicketTypes;

public record CreateTicketTypeDto(string Name, decimal Price, int TotalQuantity);
public record TicketTypeDto(Guid Id, Guid EventId, string Name, decimal Price, int TotalQuantity, int AvailableQuantity);
