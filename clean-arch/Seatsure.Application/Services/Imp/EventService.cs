using Seatsure.Application.Bll.DTOs;
using Seatsure.Application.DTOs;
using Seatsure.Application.DTOs.TicketTypes;
using Seatsure.Application.Exceptions;
using Seatsure.Application.Interfaces;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;

namespace Seatsure.Application.Services.Imp;

public sealed class EventService : IEventService
{
    private const int MaxPageSize = 100;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public EventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork; _eventRepository = eventRepository; _userRepository = userRepository;
    }

    public async Task<EventDto> CreateEventAsync(Guid callerId, CreateEventDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new ValidationException("Description is required.");
        if (string.IsNullOrWhiteSpace(request.Venue)) throw new ValidationException("Venue is required.");
        if (request.StartsAtUtc <= DateTime.UtcNow) throw new ValidationException("Start time must be in the future.");

        var organizer = await _userRepository.GetByIdAsync(callerId) ?? throw new NotFoundException("User not found.");
        if (organizer.Role != UserRole.Organizer) throw new ForbiddenException("Only organizers can create events.");

        var ev = new Event
        {
            OrganizerId = callerId, Title = request.Title.Trim(), Description = request.Description.Trim(),
            VenueName = request.Venue.Trim(), StartsAtUtc = request.StartsAtUtc, Status = EventStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _eventRepository.AddAsync(ev);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(ev);
    }

    public async Task<EventDetailsDto> GetEventByIdAsync(Guid id)
    {
        var ev = await _eventRepository.GetByIdAsync(id) ?? throw new NotFoundException("Event not found.");
        return new EventDetailsDto(ev.Id, ev.OrganizerId, ev.Title, ev.Description, ev.VenueName, ev.StartsAtUtc, ev.Status,
            ev.TicketTypes.Select(t => new TicketTypeDto(t.Id, t.EventId, t.Name, t.Price, t.TotalQuantity, t.AvailableQuantity)).ToList());
    }

    public async Task<PagedResult<EventDto>> ListPublishedEventsAsync(int page, int pageSize)
    {
        if (page < 1) throw new ValidationException("Page must be at least 1.");
        if (pageSize < 1 || pageSize > MaxPageSize) throw new ValidationException($"Page size must be between 1 and {MaxPageSize}.");
        var (items, totalCount) = await _eventRepository.GetPublishedAsync(page, pageSize);
        return new PagedResult<EventDto>(items.Select(ToDto).ToList(), page, pageSize, totalCount);
    }

    public async Task<EventDto> PublishEventAsync(Guid callerId, Guid eventId)
    {
        var ev = await _eventRepository.GetByIdAsync(eventId) ?? throw new NotFoundException("Event not found.");
        if (ev.OrganizerId != callerId) throw new ForbiddenException("You do not own this event.");
        if (ev.Status == EventStatus.Published) throw new ConflictException("Event is already published.");
        if (ev.Status == EventStatus.Cancelled) throw new ConflictException("Cancelled events cannot be published.");
        ev.Status = EventStatus.Published;
        await _unitOfWork.SaveChangesAsync();
        return ToDto(ev);
    }

    private static EventDto ToDto(Event ev) => new(ev.Id, ev.OrganizerId, ev.Title, ev.Description, ev.VenueName, ev.StartsAtUtc, ev.Status);
}
