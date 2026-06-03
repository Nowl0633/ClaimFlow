using Registration.DTOs;

namespace Registration.Services
{
    public interface IRegistrationService
    {
        Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
