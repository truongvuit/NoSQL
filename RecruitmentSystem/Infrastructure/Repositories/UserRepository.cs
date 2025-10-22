using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        public UserRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users.Find(u => u.Id == id && u.DeletedAt == null).FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.Find(u => u.Email == email && u.DeletedAt == null).FirstOrDefaultAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdateAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<User>.Update.Set(u => u.DeletedAt, DateTime.UtcNow);
            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }
        

        
        public async Task<bool> UpdateUserRoleAsync(string userId, string role)
        {
            var update = Builders<User>.Update
                .Set(u => u.Role, role)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
                
            var result = await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }
        
        public async Task<List<User>> GetPendingCompaniesAsync(int page, int pageSize)
        {
            return await _context.Users
                .Find(u => u.Company != null && u.Company.Verified == false && u.Company.IsActive == false)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountPendingCompaniesAsync()
        {
            return await _context.Users
                .CountDocumentsAsync(u => u.Company != null && u.Company.Verified == false && u.Company.IsActive == false);
        }
        
        public async Task<bool> ApproveCompanyAsync(string companyId, bool approved, string rejectionReason = null)
        {
            var user = await _context.Users.Find(u => u.Company != null && u.Company.Id == companyId).FirstOrDefaultAsync();

            if (user == null)
            {
                return false; // User or company not found
            }

            if (approved)
            {
                user.Company.Verified = true;
                user.Company.IsActive = true;
                user.Company.VerifiedAt = DateTime.UtcNow;
                
                // Also update user role
                await UpdateUserRoleAsync(user.Id, "recruiter");
            }
            else
            {
                // If registration is denied, remove the embedded company
                user.Company = null;
            }

            return await UpdateAsync(user.Id, user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var count = await _context.Users.CountDocumentsAsync(u => u.Email == email && u.DeletedAt == null);
            return count > 0;
        }

        public async Task<List<User>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Users
                .Find(u => u.DeletedAt == null)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateLastLoginAsync(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.LastLoginAt, DateTime.UtcNow)
                .Set(u => u.LoginAttempts, 0);
            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<long> CountUsersByRoleInRangeAsync(string role, DateTime start, DateTime end)
        {
            var filterBuilder = Builders<User>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(u => u.DeletedAt, null),
                filterBuilder.Eq(u => u.Role, role),
                filterBuilder.Gte(u => u.CreatedAt, start),
                filterBuilder.Lte(u => u.CreatedAt, end)
            );

            return await _context.Users.CountDocumentsAsync(filter);
        }
    }
}
