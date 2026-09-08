using Seatsure.Application.DTOs.TicketTypes;

namespace Seatsure.Application.Services.Interfaces;

public interface ITicketTypeService
{
    Task<IEnumerable<TicketTypeDto>> ListTicketTypesForEventAsync(Guid eventId);
    Task<TicketTypeDto> AddAsync(Guid callerId, Guid eventId, CreateTicketTypeDto request);
}
