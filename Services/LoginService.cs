using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClaimFlow.Data;
using ClaimFlow.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClaimFlow.Services;

public class LoginService : LoginServiceInterface
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public LoginService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email.ToLower().Trim());

        if (customer == null)
            return null;

        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.Password);
        if (!passwordValid)
            return null;

        var token = GenerateToken(customer.Id, customer.Email);

        return new LoginResponse
        {
            Token = token,
            CustomerId = customer.Id,
            Name = $"{customer.FirstName} {customer.LastName}",
            Email = customer.Email
        };
    }

    private string GenerateToken(Guid customerId, string email)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
