namespace ClaimFlow.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid ClaimId { get; set; }
        public Guid CustomerId { get; set; }
        public string FileName { get; set; } = "";
        public string StoredName { get; set; } = "";
        public long FileSize { get; set; }
        public string FileType { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}
