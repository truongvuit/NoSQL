using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Job
{
    public class JobDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public CompanySnapshotDto CompanySnapshot { get; set; }
        public SalaryDto Salary { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public string EmploymentType { get; set; }
        public string WorkMode { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Categories { get; set; }
        public string JobDetails { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public WorkplaceDto Workplace { get; set; }
        public string Status { get; set; }
        public int Views { get; set; }
        public int ApplicationCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EndDate { get; set; }
    }

    public class CompanySnapshotDto
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Tier { get; set; }
    }

    public class SalaryDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
    }

    public class WorkplaceDto
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
    }

    public class CreateJobRequest
    {
        public string Title { get; set; }
        public SalaryDto Salary { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public string EmploymentType { get; set; }
        public string WorkMode { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Categories { get; set; }
        public string JobDetails { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public WorkplaceDto Workplace { get; set; }
        public int Vacancies { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class JobSearchRequest
    {
        public string Keyword { get; set; }
        public string City { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Skills { get; set; } = new List<string>();
        public string? WorkMode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ApplyJobRequest
    {
        public string CoverLetter { get; set; }
        public string ResumeUrl { get; set; }
    }

    public class UpdateApplicationStatusRequest
    {
        public string Status { get; set; }
        public string Note { get; set; }
    }

    public class UpdateJobRequest
    {
        public string? Title { get; set; }
        public SalaryDto? Salary { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkMode { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Categories { get; set; }
        public string? JobDetails { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public WorkplaceDto? Workplace { get; set; }
        public int? Vacancies { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
