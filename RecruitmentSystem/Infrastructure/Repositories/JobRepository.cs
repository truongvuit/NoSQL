using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly MongoDbContext _context;

        public JobRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Job> GetByIdAsync(string id)
        {
            return await _context.Jobs.Find(j => j.Id == id && j.DeletedAt == null).FirstOrDefaultAsync();
        }

        public async Task<List<Job>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Jobs
                .Find(j => j.DeletedAt == null)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountAllAsync()
        {
            return await _context.Jobs
                .Find(j => j.DeletedAt == null)
                .CountDocumentsAsync();
        }

        public async Task<Job> CreateAsync(Job job)
        {
            await _context.Jobs.InsertOneAsync(job);
            return job;
        }

        public async Task<bool> UpdateAsync(string id, Job job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Jobs.ReplaceOneAsync(j => j.Id == id, job);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<Job>.Update.Set(j => j.DeletedAt, DateTime.UtcNow);
            var result = await _context.Jobs.UpdateOneAsync(j => j.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Job>> SearchJobsAsync(string keyword, string city, List<string> categories, int page, int pageSize)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filters = new List<FilterDefinition<Job>>
            {
                filterBuilder.Eq(j => j.DeletedAt, null),
                filterBuilder.Eq(j => j.Status, "published")
            };

            if (!string.IsNullOrEmpty(keyword))
            {
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.AnyIn(j => j.Keywords, new[] { keyword })
                ));
            }

            if (!string.IsNullOrEmpty(city))
            {
                filters.Add(filterBuilder.Eq(j => j.Workplace.City, city));
            }

            if (categories != null && categories.Any())
            {
                filters.Add(filterBuilder.AnyIn(j => j.Categories, categories));
            }

            var filter = filterBuilder.And(filters);

            return await _context.Jobs
                .Find(filter)
                .SortByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountSearchJobsAsync(string keyword, string city, List<string> categories)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filters = new List<FilterDefinition<Job>>
            {
                filterBuilder.Eq(j => j.DeletedAt, null),
                filterBuilder.Eq(j => j.Status, "published")
            };

            if (!string.IsNullOrEmpty(keyword))
            {
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.AnyIn(j => j.Keywords, new[] { keyword })
                ));
            }

            if (!string.IsNullOrEmpty(city))
            {
                filters.Add(filterBuilder.Eq(j => j.Workplace.City, city));
            }

            if (categories != null && categories.Any())
            {
                filters.Add(filterBuilder.AnyIn(j => j.Categories, categories));
            }

            var filter = filterBuilder.And(filters);

            return await _context.Jobs
                .Find(filter)
                .CountDocumentsAsync();
        }

        public async Task<bool> AddApplicantAsync(string jobId, Applicant applicant)
        {
            var update = Builders<Job>.Update
                .Push(j => j.Applicants, applicant)
                .Inc(j => j.ApplicationCount, 1);

            var result = await _context.Jobs.UpdateOneAsync(j => j.Id == jobId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateApplicationStatusAsync(string jobId, string applicantId, string status, string note, string changedBy)
        {
            var filter = Builders<Job>.Filter.And(
                Builders<Job>.Filter.Eq(j => j.Id, jobId),
                Builders<Job>.Filter.ElemMatch(j => j.Applicants, a => a.Id == applicantId)
            );

            var statusHistory = new StatusHistory
            {
                Status = status,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = changedBy,
                Note = note
            };

            var update = Builders<Job>.Update
                .Set("applicants.$.status", status)
                .Set("applicants.$.updatedAt", DateTime.UtcNow)
                .Push("applicants.$.statusHistory", statusHistory);

            var result = await _context.Jobs.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> IncrementViewsAsync(string jobId)
        {
            var update = Builders<Job>.Update.Inc(j => j.Views, 1);
            var result = await _context.Jobs.UpdateOneAsync(j => j.Id == jobId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Job>> GetJobsByCompanyAsync(string companyId, int page, int pageSize)
        {
            return await _context.Jobs
                .Find(j => j.CompanyId == companyId && j.DeletedAt == null)
                .SortByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountJobsByCompanyAsync(string companyId)
        {
            return await _context.Jobs
                .Find(j => j.CompanyId == companyId && j.DeletedAt == null)
                .CountDocumentsAsync();
        }

        public async Task<long> CountJobsInRangeAsync(DateTime start, DateTime end)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(j => j.DeletedAt, null),
                filterBuilder.Gte(j => j.CreatedAt, start),
                filterBuilder.Lte(j => j.CreatedAt, end)
            );

            return await _context.Jobs.CountDocumentsAsync(filter);
        }

        public async Task<List<Job>> GetAllAsyncUnpaged()
        {
            return await _context.Jobs
                .Find(j => j.DeletedAt == null)
                .ToListAsync();
        }
    }
}
