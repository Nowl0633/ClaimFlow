using Microsoft.AspNetCore.Mvc;
using Moq;
using Registration.Controllers;
using Registration.DTOs;
using Registration.Services;

namespace ClaimFlow.Tests.Controllers
{
    public class CustomersControllerTests
    {
        [Fact]
        public async Task Register_Success_Returns200()
        {
            // Arrange
            var req = new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var mockSvc = new Mock<IRegistrationService>();
            mockSvc.Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ReturnsAsync(new RegisterCustomerResponse
                {
                    CustomerId = Guid.NewGuid(),
                    Email = "jane@example.com",
                    Message = "Registration successful."
                });

            var controller = new CustomersController(mockSvc.Object);

            // Act
            var result = await controller.Register(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<RegisterCustomerResponse>(ok.Value);
            Assert.Equal("jane@example.com", body.Email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            var mockSvc = new Mock<IRegistrationService>();
            mockSvc
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ThrowsAsync(new InvalidOperationException("Email already registered."));

            var controller = new CustomersController(mockSvc.Object);

            var result = await controller.Register(new RegisterCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateTime(1990, 1, 1)
            });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_CallsServiceOnce()
        {
            var svc = new Mock<IRegistrationService>();
            svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ReturnsAsync(new RegisterCustomerResponse());

            var controller = new CustomersController(svc.Object);

            await controller.Register(new RegisterCustomerRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "abc123",
                DateOfBirth = new DateTime(2000, 1, 1)
            });

            svc.Verify(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()), Times.Once);
        }
    }
}
