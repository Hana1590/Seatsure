using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Seatsure.Domain;

namespace Seatsure.Application.Security;

public sealed class JwtService : IJWTService
{
    private readonly Jwtoptions _jwtoptions;
    public JwtService(IOptions<Jwtoptions> jwtoptions) => _jwtoptions = jwtoptions.Value;

    public (string token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtoptions.Key) || Encoding.UTF8.GetByteCount(_jwtoptions.Key) < 32)
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");

        var expires = DateTime.UtcNow.AddMinutes(_jwtoptions.ExpiryMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString())
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtoptions.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtoptions.Issuer,
            audience: _jwtoptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
