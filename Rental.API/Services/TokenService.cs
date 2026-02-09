using Microsoft.IdentityModel.Tokens;
using Rental.API.Models.Responses;
using Rental.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Rental.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;
        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecurityKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string userId)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(configuration["JwtSettings:RefreshTokenExpiryInDays"]!)),
                RevokedAt = null,
                ReplacedByToken = null
            };
        }
    }
}
