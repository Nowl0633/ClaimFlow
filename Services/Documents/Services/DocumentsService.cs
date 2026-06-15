using ClaimFlow.Data;
using ClaimFlow.Models;
using ClaimFlow.Services;
using Documents.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Documents.Services
{
    public class DocumentsService : IDocumentsService
    {
        private readonly AppDbContext _db;
        private readonly string _uploadPath;
        private readonly IAuditService? _audit;

        public DocumentsService(AppDbContext db, IAuditService? audit = null)
        {
            _db = db;
            _audit = audit;

            // put uploads in the working dir - good enough for now
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<DocumentResponse> UploadAsync(Guid customerId, Guid claimId, IFormFile file)
        {
            var claim = await _db.Claims
                .FirstOrDefaultAsync(c => c.Id == claimId && c.CustomerId == customerId);

            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            // 10mb limit, files bigger than this probably shouldnt be going in a db anyway
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File is too large, max 10MB.");

            var ext = Path.GetExtension(file.FileName);
            var storedAs = Guid.NewGuid().ToString() + ext;
            var fullPath = Path.Combine(_uploadPath, storedAs);

            using (var stream = File.Create(fullPath))
                await file.CopyToAsync(stream);

            var doc = new Document
            {
                Id = Guid.NewGuid(),
                ClaimId = claimId,
                CustomerId = customerId,
                FileName = file.FileName,
                StoredName = storedAs,
                FileSize = file.Length,
                FileType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            if (_audit != null)
                await _audit.LogAsync("DocumentUploaded", "Document", doc.Id, customerId);

            return BuildDocResponse(doc);
        }

        public async Task<List<DocumentResponse>> GetForClaimAsync(Guid claimId, Guid customerId)
        {
            // verify ownership first
            bool exists = await _db.Claims
                .AnyAsync(c => c.Id == claimId && c.CustomerId == customerId);

            if (!exists)
                throw new KeyNotFoundException("Claim not found.");

            var docs = await _db.Documents
                .Where(d => d.ClaimId == claimId)
                .OrderBy(d => d.UploadedAt)
                .ToListAsync();

            return docs.Select(d => BuildDocResponse(d)).ToList();
        }

        public async Task<(string path, string fileName)> GetFileAsync(Guid documentId, Guid customerId)
        {
            var doc = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId);

            if (doc == null)
                throw new KeyNotFoundException("Document not found.");

            var path = Path.Combine(_uploadPath, doc.StoredName);

            if (!File.Exists(path))
                throw new FileNotFoundException("File missing from storage.");

            return (path, doc.FileName);
        }

        public async Task DeleteAsync(Guid documentId, Guid customerId)
        {
            var doc = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId);

            if (doc == null)
                throw new KeyNotFoundException("Document not found.");

            var path = Path.Combine(_uploadPath, doc.StoredName);
            if (File.Exists(path))
                File.Delete(path);

            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync();

            if (_audit != null)
                await _audit.LogAsync("DocumentDeleted", "Document", doc.Id, customerId);
        }

        private DocumentResponse BuildDocResponse(Document d)
        {
            return new DocumentResponse
            {
                DocumentId = d.Id,
                ClaimId = d.ClaimId,
                FileName = d.FileName,
                FileType = d.FileType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt
            };
        }
    }
}
