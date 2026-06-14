using ClaimFlow.Data;
using ClaimFlow.Models;
using Documents.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace ClaimFlow.Tests.Documents
{
    public class DocumentsServiceTests
    {
        private static AppDbContext GetDb() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        // the service writes real files to disk so each test gets its own temp folder
        private static string MakeTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "claimflow_tests_" + Guid.NewGuid());
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static Mock<IFormFile> MakeFile(string name = "receipt.pdf", long size = 1024, string contentType = "application/pdf")
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(name);
            mock.Setup(f => f.Length).Returns(size);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        private static async Task<(AppDbContext db, Guid customerId, Guid claimId)> SeedClaim(AppDbContext? existing = null)
        {
            var db = existing ?? GetDb();
            var customerId = Guid.NewGuid();
            var claimId = Guid.NewGuid();

            db.Claims.Add(new Claim
            {
                Id = claimId,
                CustomerId = customerId,
                PolicyId = Guid.NewGuid(),
                Description = "lost luggage",
                ClaimType = "Baggage",
                Amount = 300m,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return (db, customerId, claimId);
        }

        [Fact]
        public async Task Upload_ValidClaim_SavesRecord()
        {
            var (db, customerId, claimId) = await SeedClaim();
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();

            try
            {
                // redirect current dir so the service drops files in our temp folder
                Directory.SetCurrentDirectory(dir);

                var svc = new DocumentsService(db);
                var result = await svc.UploadAsync(customerId, claimId, MakeFile().Object);

                Assert.Equal("receipt.pdf", result.FileName);
                Assert.Equal(claimId, result.ClaimId);
                Assert.Equal(1024, result.FileSize);
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task Upload_ClaimNotFound_Throws()
        {
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                var svc = new DocumentsService(GetDb());

                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => svc.UploadAsync(Guid.NewGuid(), Guid.NewGuid(), MakeFile().Object));
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task Upload_FileTooLarge_Throws()
        {
            var (db, customerId, claimId) = await SeedClaim();
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                var svc = new DocumentsService(db);

                // just over 10mb
                var bigFile = MakeFile(size: 11 * 1024 * 1024);

                await Assert.ThrowsAsync<ArgumentException>(
                    () => svc.UploadAsync(customerId, claimId, bigFile.Object));
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task GetForClaim_ReturnsDocuments()
        {
            var (db, customerId, claimId) = await SeedClaim();

            db.Documents.Add(new Document { Id = Guid.NewGuid(), ClaimId = claimId, CustomerId = customerId, FileName = "a.pdf", StoredName = "a.pdf", UploadedAt = DateTime.UtcNow });
            db.Documents.Add(new Document { Id = Guid.NewGuid(), ClaimId = claimId, CustomerId = customerId, FileName = "b.pdf", StoredName = "b.pdf", UploadedAt = DateTime.UtcNow.AddSeconds(1) });
            await db.SaveChangesAsync();

            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dir);

            try
            {
                var svc = new DocumentsService(db);
                var results = await svc.GetForClaimAsync(claimId, customerId);
                Assert.Equal(2, results.Count);
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task GetForClaim_WrongCustomer_Throws()
        {
            var (db, _, claimId) = await SeedClaim();
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dir);

            try
            {
                var svc = new DocumentsService(db);

                // different customer id - shouldn't be able to see these docs
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => svc.GetForClaimAsync(claimId, Guid.NewGuid()));
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task Delete_RemovesRecord()
        {
            var (db, customerId, claimId) = await SeedClaim();
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dir);

            try
            {
                var svc = new DocumentsService(db);

                var uploadsDir = Path.Combine(dir, "uploads");
                Directory.CreateDirectory(uploadsDir);

                var storedName = Guid.NewGuid() + ".pdf";
                File.WriteAllText(Path.Combine(uploadsDir, storedName), "fake");

                var docId = Guid.NewGuid();
                db.Documents.Add(new Document
                {
                    Id = docId,
                    ClaimId = claimId,
                    CustomerId = customerId,
                    FileName = "original.pdf",
                    StoredName = storedName,
                    UploadedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                await svc.DeleteAsync(docId, customerId);

                var gone = await db.Documents.FindAsync(docId);
                Assert.Null(gone);
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public async Task Delete_WrongCustomer_Throws()
        {
            var (db, customerId, claimId) = await SeedClaim();
            var dir = MakeTempDir();
            var old = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dir);

            try
            {
                var svc = new DocumentsService(db);

                var docId = Guid.NewGuid();
                db.Documents.Add(new Document
                {
                    Id = docId,
                    ClaimId = claimId,
                    CustomerId = customerId,
                    FileName = "receipt.pdf",
                    StoredName = "whatever.pdf",
                    UploadedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => svc.DeleteAsync(docId, Guid.NewGuid()));
            }
            finally
            {
                Directory.SetCurrentDirectory(old);
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
