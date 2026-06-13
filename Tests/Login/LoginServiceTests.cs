using ClaimFlow.Data;
using ClaimFlow.Models;
using Login.DTOs;
using Login.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ClaimFlow.Tests.Login
{
    public class LoginServiceTests
    {
        private static AppDbContext CreateContext() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IConfiguration FakeConfig()
        {
            var mock = new Mock<IConfiguration>();
            mock.Setup(c => c["Jwt:Key"]).Returns("claimflow-test-secret-key-long-enough");
            mock.Setup(c => c["Jwt:Issuer"]).Returns("ClaimFlow");
            mock.Setup(c => c["Jwt:Audience"]).Returns("ClaimFlow");
            return mock.Object;
        }

        private static async Task<AppDbContext> ContextWithCustomer()
        {
            var context = CreateContext();
            context.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                // work factor 4 so tests dont take ages
                Password = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 4),
                DateOfBirth = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var service = new LoginService(await ContextWithCustomer(), FakeConfig());

            var result = await service.LoginAsync(new LoginRequest
            {
                Email = "jane@example.com",
                Password = "password123"
            });

            Assert.NotEmpty(result.Token);
            Assert.Equal("Jane", result.Name);
            Assert.Equal("jane@example.com", result.Email);
        }

        [Fact]
        public async Task Login_WrongPassword_Throws()
        {
            var service = new LoginService(await ContextWithCustomer(), FakeConfig());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.LoginAsync(new LoginRequest
                {
                    Email = "jane@example.com",
                    Password = "wrongpassword"
                }));
        }

        [Fact]
        public async Task Login_UnknownEmail_Throws()
        {
            var service = new LoginService(CreateContext(), FakeConfig());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.LoginAsync(new LoginRequest
                {
                    Email = "nobody@example.com",
                    Password = "password123"
                }));
        }
    }
}
