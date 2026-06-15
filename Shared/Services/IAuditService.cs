namespace ClaimFlow.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, Guid? entityId = null, Guid? customerId = null);
    }
}
