using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatsure.Application.DTOs.Reservation;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Seatsure.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    public ReservationsController(IReservationService service) => _service = service;
    private Guid CallerId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("ticket-types/{id:guid}/reservations")]
    public async Task<IActionResult> Create(Guid id, CreateReservationDto request)
        => StatusCode(StatusCodes.Status201Created, await _service.CreateHoldAsync(CallerId, id, request));

    [HttpPost("reservations/{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id) => Ok(await _service.ConfirmAsync(CallerId, id));

    [HttpPost("reservations/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id) => Ok(await _service.CancelAsync(CallerId, id));

    [HttpGet("users/me/reservations")]
    public async Task<IActionResult> Mine() => Ok(await _service.GetMyReservationsAsync(CallerId));
}
