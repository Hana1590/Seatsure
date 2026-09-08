

using Seatsure.Application.DTOs.Auth;
namespace Seatsure.Application.Services.Interfaces;

public interface IAuthService
{

    Task<UserDto> Register(RegisterRequest request);

    Task<AuthResult> Login(LoginRequest request);


}