using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectionApi.Net.Data;
using ElectionApi.Net.Models;
using BCrypt.Net;

namespace ElectionApi.Net.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<Admin> _adminRepository;
    private readonly IRepository<Voter> _voterRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IRepository<Admin> adminRepository,
        IRepository<Voter> voterRepository,
        IConfiguration configuration)
    {
        _adminRepository = adminRepository;
        _voterRepository = voterRepository;
        _configuration = configuration;
    }

    public async Task<Admin?> ValidateAdminCredentialsAsync(string email, string password)
    {
        var admin = await _adminRepository.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
        
        if (admin == null || !VerifyPassword(password, admin.Password))
            return null;

        return admin;
    }

    public async Task<Voter?> ValidateVoterCredentialsAsync(string email, string password)
    {
        var voter = await _voterRepository.FirstOrDefaultAsync(v => v.Email == email && v.IsActive && v.IsVerified);
        
        if (voter == null || !VerifyPassword(password, voter.Password))
            return null;

        return voter;
    }

    public async Task<string> GenerateJwtTokenAsync(int userId, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expireMinutes = role == "admin" ? 59 : 10; // Admin: 59 minutes, Voter: 10 minutes
        var expiry = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("user_id", userId.ToString()),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return validatedToken != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task UpdateLastLoginAsync(int userId, string userType)
    {
        var now = DateTime.UtcNow;
        var ipAddress = ""; // This should be set from HttpContext in controller

        if (userType == "admin")
        {
            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin != null)
            {
                admin.LastLoginAt = now;
                admin.LastLoginIp = ipAddress;
                await _adminRepository.UpdateAsync(admin);
            }
        }
        else if (userType == "voter")
        {
            var voter = await _voterRepository.GetByIdAsync(userId);
            if (voter != null)
            {
                voter.LastLoginAt = now;
                voter.LastLoginIp = ipAddress;
                await _voterRepository.UpdateAsync(voter);
            }
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}