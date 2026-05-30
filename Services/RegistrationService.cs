using ClaimFlow.Data;
using ClaimFlow.DTOs;
using ClaimFlow.Models;

namespace ClaimFlow.Services;

public class RegistrationService : IRegistrationService
{
    private readonly AppDbContext _db;

    public RegistrationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> EmailExists(string email)
    {
        return _db.Customers.Any(c => c.Email == email);
    }

    public async Task<RegisterCustomerResponse> Register(RegisterCustomerRequest request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower().Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return new RegisterCustomerResponse
        {
            Id = customer.Id,
            Name = $"{customer.FirstName} {customer.LastName}",
            Email = customer.Email,
            Message = "Account created successfully"
        };
    }
}