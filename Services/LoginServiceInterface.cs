using ClaimFlow.DTOs;

namespace ClaimFlow.Services;

public interface LoginServiceInterface
{
    Task<LoginResponse?> Login(LoginRequest request);
}
