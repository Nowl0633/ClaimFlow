namespace ClaimFlow.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }  // nullable - some events arent tied to a user
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public Guid? EntityId { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
