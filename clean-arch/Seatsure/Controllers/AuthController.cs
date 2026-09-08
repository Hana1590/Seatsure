using Microsoft.AspNetCore.Mvc;
using Seatsure.Application.DTOs.Auth;
using Seatsure.Application.Services.Interfaces;

namespace Seatsure.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    public AuthController(IAuthService service) => _service = service;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request) => StatusCode(StatusCodes.Status201Created, await _service.Register(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request) => Ok(await _service.Login(request));
}
