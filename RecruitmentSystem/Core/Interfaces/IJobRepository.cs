using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> GetByIdAsync(string id);
        Task<List<Job>> GetAllAsync(int page, int pageSize);
        Task<Job> CreateAsync(Job job);
        Task<bool> UpdateAsync(string id, Job job);
        Task<bool> DeleteAsync(string id);
        Task<List<Job>> SearchJobsAsync(string keyword, string city, List<string> categories, int page, int pageSize);
        Task<bool> AddApplicantAsync(string jobId, Applicant applicant);
        Task<bool> UpdateApplicationStatusAsync(string jobId, string applicantId, string status, string note, string changedBy);
        Task<bool> IncrementViewsAsync(string jobId);
        Task<List<Job>> GetJobsByCompanyAsync(string companyId, int page, int pageSize);
        Task<long> CountJobsInRangeAsync(DateTime start, DateTime end);
        Task<List<Job>> GetAllAsyncUnpaged();
    }
}
