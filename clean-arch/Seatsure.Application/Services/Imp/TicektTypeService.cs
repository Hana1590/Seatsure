using Seatsure.Application.DTOs.TicketTypes;
using Seatsure.Application.Exceptions;
using Seatsure.Application.Interfaces;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;

namespace Seatsure.Application.Services.Imp;

public sealed class TicektTypeService : ITicketTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly IEventRepository _eventRepository;
    public TicektTypeService(IUnitOfWork unitOfWork, ITicketTypeRepository ticketTypeRepository, IEventRepository eventRepository)
    { _unitOfWork = unitOfWork; _ticketTypeRepository = ticketTypeRepository; _eventRepository = eventRepository; }

    public async Task<TicketTypeDto> AddAsync(Guid callerId, Guid eventId, CreateTicketTypeDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("Ticket type name is required.");
        if (request.Price <= 0) throw new ValidationException("Price must be greater than zero.");
        if (request.TotalQuantity < 1) throw new ValidationException("Total quantity must be at least 1.");

        var ev = await _eventRepository.GetByIdAsync(eventId) ?? throw new NotFoundException("Event not found.");
        if (ev.OrganizerId != callerId) throw new ForbiddenException("You do not own this event.");

        var ticket = new TicketType { EventId = eventId, Name = request.Name.Trim(), Price = request.Price,
            TotalQuantity = request.TotalQuantity, AvailableQuantity = request.TotalQuantity };
        await _ticketTypeRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(ticket);
    }

    public async Task<IEnumerable<TicketTypeDto>> ListTicketTypesForEventAsync(Guid eventId)
    {
        _ = await _eventRepository.GetByIdAsync(eventId) ?? throw new NotFoundException("Event not found.");
        var tickets = await _ticketTypeRepository.GetByEventIdAsync(eventId);
        return tickets.Select(ToDto).ToList();
    }

    private static TicketTypeDto ToDto(TicketType t) => new(t.Id, t.EventId, t.Name, t.Price, t.TotalQuantity, t.AvailableQuantity);
}
