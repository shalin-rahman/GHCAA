using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Repositories
{
    public class FileUploadRepository : IFileUploadRepository
    {
        private readonly ApplicationDbContext _db;
        public FileUploadRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(FileUpload entity, CancellationToken cancellationToken = default)
        {
            await _db.FileUploads.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<FileUpload?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.FileUploads.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IEnumerable<FileUpload>> GetByMemberAsync(int memberId, CancellationToken cancellationToken = default)
            => await _db.FileUploads.Where(f => f.MemberId == memberId).ToListAsync(cancellationToken);

        public async Task UpdateAsync(FileUpload entity, CancellationToken cancellationToken = default)
        {
            _db.FileUploads.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(FileUpload entity, CancellationToken cancellationToken = default)
        {
            _db.FileUploads.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
