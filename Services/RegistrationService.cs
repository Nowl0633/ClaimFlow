using ClaimFlow.Data;
using ClaimFlow.DTOs;
using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Services;

public class RegistrationService : RegistrationServiceInterface
{
    private readonly AppDbContext _db;

    public RegistrationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> EmailExists(string email)
    {
        return await _db.Customers.AnyAsync(c => c.Email == email.ToLower().Trim());
    }

    public async Task<RegisterCustomerResponse> Register(RegisterCustomerRequest request)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower().Trim(),
            Password = hashedPassword,
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
