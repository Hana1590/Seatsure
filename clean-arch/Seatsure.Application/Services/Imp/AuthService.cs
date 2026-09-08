using System.Net.Mail;
using Seatsure.Application.DTOs.Auth;
using Seatsure.Application.Exceptions;
using Seatsure.Application.Interfaces;
using Seatsure.Application.Security;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Domain;

namespace Seatsure.Application.Services.Imp;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJWTService _jwtService;

    public AuthService(IUserRepository users, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJWTService jwtService)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<UserDto> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ValidationException("Email is required.");
        if (!MailAddress.TryCreate(request.Email.Trim(), out var mail) || mail.Address != request.Email.Trim())
            throw new ValidationException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ValidationException("Password is required.");
        if (request.Password.Length < 6) throw new ValidationException("Password must be at least 6 characters.");
        if (!Enum.IsDefined(request.Role)) throw new ValidationException("Invalid user role.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.GetByEmailAsync(email) is not null)
            throw new EmailTakenException("Email is already registered.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserDto(user.Id, user.Name, user.Email, user.Role);
    }

    public async Task<AuthResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !_passwordHasher.verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var (token, expiresAtUtc) = _jwtService.GenerateToken(user);
        return new AuthResult(token, expiresAtUtc);
    }
}
