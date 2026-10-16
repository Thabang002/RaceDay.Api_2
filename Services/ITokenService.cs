using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models.Entities;

namespace RaceDay.Api.Services;

public interface ITokenService { string GenerateToken(User user); }


