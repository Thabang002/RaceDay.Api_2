using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models.Entities;

namespace RaceDay.Api.Services;



public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string GenerateToken(User user)
    {
        var claims = new Dictionary<string, object>
        {
            [ClaimTypes.NameIdentifier] = user.UserId.ToString(),
            [ClaimTypes.Email] = user.Email,
            [ClaimTypes.Name] = user.FullName,
            [ClaimTypes.Role] = user.Role
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            SigningCredentials = creds,
            Claims = claims
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}