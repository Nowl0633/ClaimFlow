using Documents.DTOs;
using Microsoft.AspNetCore.Http;

namespace Documents.Services
{
    public interface IDocumentsService
    {
        Task<DocumentResponse> UploadAsync(Guid customerId, Guid claimId, IFormFile file);
        Task<List<DocumentResponse>> GetForClaimAsync(Guid claimId, Guid customerId);
        Task<(string path, string fileName)> GetFileAsync(Guid documentId, Guid customerId);
        Task DeleteAsync(Guid documentId, Guid customerId);
    }
}
