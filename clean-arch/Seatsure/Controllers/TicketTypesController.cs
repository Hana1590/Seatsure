using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatsure.Application.DTOs.TicketTypes;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Seatsure.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/ticket-types")]
public sealed class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _service;
    public TicketTypesController(ITicketTypeService service) => _service = service;
    private Guid CallerId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(Guid eventId) => Ok(await _service.ListTicketTypesForEventAsync(eventId));

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Organizer))]
    public async Task<IActionResult> Add(Guid eventId, CreateTicketTypeDto request)
    {
        var result = await _service.AddAsync(CallerId, eventId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
