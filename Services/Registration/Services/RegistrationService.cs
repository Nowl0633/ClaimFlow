using ClaimFlow.Data;
using ClaimFlow.Models;
using ClaimFlow.Services;
using Microsoft.EntityFrameworkCore;
using Registration.DTOs;

namespace Registration.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService? _email;
        private readonly IAuditService? _audit;

        public RegistrationService(AppDbContext context, IEmailService? email = null, IAuditService? audit = null)
        {
            _context = context;
            _email = email;
            _audit = audit;
        }

        public async Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request)
        {
            bool emailTaken = await _context.Customers.AnyAsync(c => c.Email == request.Email);
            if (emailTaken)
                throw new InvalidOperationException("Email already registered.");

            var id = Guid.NewGuid();
            var customer = new Customer
            {
                Id = id,
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

            if (_audit != null)
                await _audit.LogAsync("Registered", "Customer", customer.Id, customer.Id);

            if (_email != null)
                await _email.SendAsync(
                    customer.Email,
                    "Welcome to ClaimFlow",
                    $"Hi {customer.FirstName},\n\nYour account has been created. You can now log in and get a quote.\n\nClaimFlow"
                );

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

            if (_audit != null)
                await _audit.LogAsync("PasswordReset", "Customer", customer.Id, customer.Id);
        }
    }
}
