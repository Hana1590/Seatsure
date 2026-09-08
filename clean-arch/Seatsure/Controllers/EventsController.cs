using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatsure.Application.DTOs;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Seatsure.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _service;
    public EventsController(IEventService service) => _service = service;
    private Guid CallerId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _service.ListPublishedEventsAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id) => Ok(await _service.GetEventByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Organizer))]
    public async Task<IActionResult> Create(CreateEventDto request) => await CreateAndReturn(request);

    private async Task<IActionResult> CreateAndReturn(CreateEventDto request)
    {
        var result = await _service.CreateEventAsync(CallerId, request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = nameof(UserRole.Organizer))]
    public async Task<IActionResult> Publish(Guid id) => Ok(await _service.PublishEventAsync(CallerId, id));
}
