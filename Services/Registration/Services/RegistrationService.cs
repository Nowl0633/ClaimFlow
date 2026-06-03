using ClaimFlow.Data;
using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;
using Registration.DTOs;

namespace Registration.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AppDbContext _context;

        public RegistrationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request)
        {
            bool emailTaken = await _context.Customers.AnyAsync(c => c.Email == request.Email);
            if (emailTaken)
                throw new InvalidOperationException("Email already registered.");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                // never store plain text passwords
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new RegisterCustomerResponse
            {
                CustomerId = customer.Id,
                Email = customer.Email,
                Message = "Registration successful."
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (customer == null)
                throw new KeyNotFoundException("No account found with that email.");

            // just overwrite the hash, bcrypt takes care of the rest
            customer.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
        }
    }
}
