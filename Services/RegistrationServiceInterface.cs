using ClaimFlow.DTOs;

namespace ClaimFlow.Services;

public interface RegistrationServiceInterface
{
    Task<RegisterCustomerResponse> Register(RegisterCustomerRequest request);
    Task<bool> EmailExists(string email);
}
