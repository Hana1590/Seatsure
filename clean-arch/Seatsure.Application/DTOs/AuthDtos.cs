using Seatsure.Domain;

namespace Seatsure.Application.DTOs.Auth;

public record RegisterRequest(string Name, string Email, string Password, UserRole Role);
public record LoginRequest(string Email, string Password);
public record AuthResult(string Token, DateTime ExpiresAtUtc);
public record UserDto(Guid Id, string Name, string Email, UserRole Role);
