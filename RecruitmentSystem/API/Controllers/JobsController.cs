using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Core.DTOs.Common;
using Core.DTOs.Job;
using Core.Interfaces;
using Core.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;

        public JobsController(
            IJobRepository jobRepository,
            IUserRepository userRepository,
            ICacheService cacheService)
        {
            _jobRepository = jobRepository;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<JobDto>>>> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var cacheKey = $"jobs:page:{page}:size:{pageSize}";
            var cachedJobs = await _cacheService.GetAsync<PagedResponse<JobDto>>(cacheKey);

            if (cachedJobs != null)
            {
                return Ok(new ApiResponse<PagedResponse<JobDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách công việc thành công (cached)",
                    Data = cachedJobs
                });
            }

            var jobs = await _jobRepository.GetAllAsync(page, pageSize);
            var jobDtos = jobs.Select(MapToJobDto).ToList();

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = jobDtos.Count,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(jobDtos.Count / (double)pageSize)
            };

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

            return Ok(new ApiResponse<PagedResponse<JobDto>>
            {
                Success = true,
                Message = "Lấy danh sách công việc thành công",
                Data = response
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Job>>> GetJobById(string id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound(new ApiResponse<Job>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc"
                });
            }

            await _jobRepository.IncrementViewsAsync(id);

            // Đảm bảo job.Applicants không null trước khi trả về
            if (job.Applicants == null)
            {
                job.Applicants = new List<Applicant>();
            }

            return Ok(new ApiResponse<Job>
            {
                Success = true,
                Message = "Lấy thông tin công việc thành công",
                Data = job
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Job>>> CreateJob([FromBody] CreateJobRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (role != "recruiter")
            {
                return StatusCode(403, new ApiResponse<Job>
                {
                    Success = false,
                    Message = "Bạn không có quyền thực hiện hành động này"
                });
            }          

            // Lấy thông tin user từ database nếu cần
            var user = await _userRepository.GetByIdAsync(userId);
            
            // Tạo job mới với thông tin cơ bản
            var job = new Job
            {
                CompanyId = user?.Company.Id ?? MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                CompanySnapshot = new CompanySnapshot
                {
                    Name = user?.Company?.Name ?? "Công ty mặc định",
                    LogoUrl = user?.Company?.LogoUrl ?? "https://example.com/default-logo.png",
                    Tier = user?.Company?.Tier ?? "Normal"
                },
                Title = request.Title,
                Salary = new Salary
                {
                    Min = request.Salary.Min,
                    Max = request.Salary.Max,
                    Currency = request.Salary.Currency,
                    Type = request.Salary.Type
                },
                Experience = request.Experience,
                Education = request.Education,
                EmploymentType = request.EmploymentType,
                WorkMode = request.WorkMode,
                Skills = request.Skills,
                Categories = request.Categories,
                JobDetails = request.JobDetails,
                Requirements = request.Requirements,
                Benefits = request.Benefits,
                Workplace = new Workplace
                {
                    Address = request.Workplace.Address,
                    City = request.Workplace.City,
                    District = request.Workplace.District
                },
                Vacancies = request.Vacancies,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedBy = userId,
                Status = "draft"
            };

            var created = await _jobRepository.CreateAsync(job);

            return CreatedAtAction(nameof(GetJobById), new { id = created.Id }, new ApiResponse<Job>
            {
                Success = true,
                Message = "Tạo công việc thành công",
                Data = created
            });
        }

        [HttpPatch("{id}/publish")]
        [Authorize(Roles = "recruiter")]
        public async Task<ActionResult<ApiResponse<bool>>> PublishJob(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc"
                });
            }

            if (job.CreatedBy != userId)
            {
                return Forbid();
            }

            job.Status = "published";
            job.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.UpdateAsync(id, job);

            for (int i = 1; i <= 10; i++)
            {
                await _cacheService.RemoveAsync($"jobs:page:{i}:size:20");
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Đăng công việc thành công",
                Data = true
            });
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<PagedResponse<JobDto>>>> SearchJobs([FromBody] JobSearchRequest request)
        {
            var jobs = await _jobRepository.SearchJobsAsync(
                request.Keyword,
                request.City,
                request.Categories,
                request.Page,
                request.PageSize
            );

            var jobDtos = jobs.Select(MapToJobDto).ToList();

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = jobDtos.Count,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(jobDtos.Count / (double)request.PageSize)
            };

            return Ok(new ApiResponse<PagedResponse<JobDto>>
            {
                Success = true,
                Message = "Tìm kiếm công việc thành công",
                Data = response
            });
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = "candidate")]
        public async Task<ActionResult<ApiResponse<bool>>> ApplyJob(string id, [FromBody] ApplyJobRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetByIdAsync(userId);
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc"
                });
            }

            if (job.Applicants.Any(a => a.ApplicantId == userId))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Bạn đã ứng tuyển công việc này rồi"
                });
            }

            var applicant = new Applicant
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                JobId = id,
                ApplicantId = userId,
                ApplicantSnapshot = new ApplicantSnapshot
                {
                    FullName = user.Profile?.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Avatar = user.Profile?.Avatar,
                    ResumeUrl = user.CandidateProfile?.ResumeUrl,
                    CurrentPosition = user.CandidateProfile?.CurrentPosition,
                    YearsOfExperience = user.CandidateProfile?.YearsOfExperience ?? 0
                },
                CoverLetter = request.CoverLetter,
                Status = "Pending",
                StatusHistory = new List<StatusHistory>
                {
                    new StatusHistory
                    {
                        Status = "Pending",
                        ChangedAt = DateTime.UtcNow,
                        Note = "Ứng viên nộp hồ sơ"
                    }
                },
                Screening = new Screening
                {
                    Score = 0,
                    MatchPercentage = 0,
                    Strengths = new List<string>(),
                    Weaknesses = new List<string>()
                },
                Interviews = new List<Interview>()
            };

            if (!string.IsNullOrEmpty(request.ResumeUrl))
            {
                applicant.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Type = "resume",
                        Url = request.ResumeUrl,
                        Name = "CV.pdf",
                        Size = 0
                    }
                };
            }

            await _jobRepository.AddApplicantAsync(id, applicant);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Ứng tuyển thành công",
                Data = true
            });
        }

        [HttpPatch("applications/{applicantId}/status")]
        [Authorize(Roles = "recruiter")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateApplicationStatus(
            string applicantId,
            [FromBody] UpdateApplicationStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var validStatuses = new[] { "Pending", "Screening", "Interview", "Rejected", "Hired" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Trạng thái không hợp lệ"
                });
            }

            var jobs = await _jobRepository.GetAllAsync(1, 1000);
            var job = jobs.FirstOrDefault(j => j.Applicants.Any(a => a.Id == applicantId));

            if (job == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Không tìm thấy hồ sơ ứng tuyển"
                });
            }

            if (job.CreatedBy != userId)
            {
                return Forbid();
            }

            await _jobRepository.UpdateApplicationStatusAsync(
                job.Id,
                applicantId,
                request.Status,
                request.Note,
                userId
            );

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Cập nhật trạng thái thành công",
                Data = true
            });
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<JobDto>>>> GetJobsByCompany(
            string companyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var jobs = await _jobRepository.GetJobsByCompanyAsync(companyId, page, pageSize);
            var jobDtos = jobs.Select(MapToJobDto).ToList();

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = jobDtos.Count,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(jobDtos.Count / (double)pageSize)
            };

            return Ok(new ApiResponse<PagedResponse<JobDto>>
            {
                Success = true,
                Message = "Lấy danh sách công việc của công ty thành công",
                Data = response
            });
        }

        private JobDto MapToJobDto(Job job)
        {
            return new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanySnapshot = new CompanySnapshotDto
                {
                    Name = job.CompanySnapshot?.Name,
                    LogoUrl = job.CompanySnapshot?.LogoUrl,
                    Tier = job.CompanySnapshot?.Tier
                },
                Salary = new SalaryDto
                {
                    Min = job.Salary.Min,
                    Max = job.Salary.Max,
                    Currency = job.Salary.Currency,
                    Type = job.Salary.Type
                },
                Experience = job.Experience,
                Education = job.Education,
                EmploymentType = job.EmploymentType,
                WorkMode = job.WorkMode,
                Skills = job.Skills,
                Categories = job.Categories,
                JobDetails = job.JobDetails,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                Workplace = new WorkplaceDto
                {
                    Address = job.Workplace?.Address,
                    City = job.Workplace?.City,
                    District = job.Workplace?.District
                },
                Status = job.Status,
                Views = job.Views,
                ApplicationCount = job.ApplicationCount,
                CreatedAt = job.CreatedAt,
                EndDate = job.EndDate
            };
        }
    }
}
