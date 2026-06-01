using ClaimFlow.DTOs;

namespace ClaimFlow.Services;

public interface RegistrationServiceInterface
{
    Task<bool> EmailExists(string email);
    Task<RegisterCustomerResponse> Register(RegisterCustomerRequest request);
}
