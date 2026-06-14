namespace Documents.DTOs
{
    public class DocumentResponse
    {
        public Guid DocumentId { get; set; }
        public Guid ClaimId { get; set; }
        public string FileName { get; set; } = "";
        public string FileType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
