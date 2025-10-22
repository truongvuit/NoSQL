using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<bool> UpdateAsync(string id, User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> EmailExistsAsync(string email);
        Task<List<User>> GetAllAsync(int page, int pageSize);
        Task<bool> UpdateLastLoginAsync(string id);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
        Task<List<User>> GetPendingCompaniesAsync(int page, int pageSize);
        Task<long> CountPendingCompaniesAsync();
        Task<bool> ApproveCompanyAsync(string companyId, bool approved, string rejectionReason = null);
        Task<long> CountUsersByRoleInRangeAsync(string role, DateTime start, DateTime end);
    }
}
