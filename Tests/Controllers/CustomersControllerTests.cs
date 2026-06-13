using Microsoft.AspNetCore.Mvc;
using Moq;
using Registration.Controllers;
using Registration.DTOs;
using Registration.Services;

namespace ClaimFlow.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private static RegisterCustomerRequest ValidRequest() => new()
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Password = "password123",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        [Fact]
        public async Task Register_Success_Returns200()
        {
            var mockService = new Mock<IRegistrationService>();
            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ReturnsAsync(new RegisterCustomerResponse
                {
                    CustomerId = Guid.NewGuid(),
                    Email = "jane@example.com",
                    Message = "Registration successful."
                });

            var controller = new CustomersController(mockService.Object);

            var result = await controller.Register(ValidRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<RegisterCustomerResponse>(ok.Value);
            Assert.Equal("jane@example.com", body.Email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            var mockService = new Mock<IRegistrationService>();
            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ThrowsAsync(new InvalidOperationException("Email already registered."));

            var controller = new CustomersController(mockService.Object);

            var result = await controller.Register(ValidRequest());

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_CallsServiceOnce()
        {
            var mockService = new Mock<IRegistrationService>();
            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()))
                .ReturnsAsync(new RegisterCustomerResponse());

            var controller = new CustomersController(mockService.Object);
            await controller.Register(ValidRequest());

            mockService.Verify(s => s.RegisterAsync(It.IsAny<RegisterCustomerRequest>()), Times.Once);
        }
    }
}
