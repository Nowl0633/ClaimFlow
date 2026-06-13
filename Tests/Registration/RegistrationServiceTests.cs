using ClaimFlow.Data;
using Microsoft.EntityFrameworkCore;
using Registration.DTOs;
using Registration.Services;

namespace ClaimFlow.Tests.Registration
{
    public class RegistrationServiceTests
    {
        private static AppDbContext CreateContext() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task Register_NewEmail_ReturnsSuccess()
        {
            var service = new RegistrationService(CreateContext());

            var result = await service.RegisterAsync(new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateTime(1990, 1, 1),
                Phone = "07700000000"
            });

            Assert.Equal("jane@example.com", result.Email);
            Assert.Equal("Registration successful.", result.Message);
            Assert.NotEqual(Guid.Empty, result.CustomerId);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Throws()
        {
            var context = CreateContext();
            var service = new RegistrationService(context);
            var request = new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            await service.RegisterAsync(request);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterAsync(request));
        }

        [Fact]
        public async Task Register_PasswordIsHashed()
        {
            var context = CreateContext();
            var service = new RegistrationService(context);

            await service.RegisterAsync(new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateTime(1990, 1, 1)
            });

            var saved = await context.Customers.FirstAsync();

            // plain text password should never end up in the database
            Assert.NotEqual("password123", saved.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("password123", saved.Password));
        }

        [Fact]
        public async Task ResetPassword_KnownEmail_UpdatesHash()
        {
            var context = CreateContext();
            var service = new RegistrationService(context);

            await service.RegisterAsync(new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "oldpassword",
                DateOfBirth = new DateTime(1990, 1, 1)
            });

            await service.ResetPasswordAsync(new ResetPasswordRequest
            {
                Email = "jane@example.com",
                NewPassword = "newpassword"
            });

            var saved = await context.Customers.FirstAsync();
            Assert.True(BCrypt.Net.BCrypt.Verify("newpassword", saved.Password));
            Assert.False(BCrypt.Net.BCrypt.Verify("oldpassword", saved.Password));
        }

        [Fact]
        public async Task ResetPassword_UnknownEmail_Throws()
        {
            var service = new RegistrationService(CreateContext());

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.ResetPasswordAsync(new ResetPasswordRequest
                {
                    Email = "nobody@example.com",
                    NewPassword = "newpassword"
                }));
        }
    }
}
