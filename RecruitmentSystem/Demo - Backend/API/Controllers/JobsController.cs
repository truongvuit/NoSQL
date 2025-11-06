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
            // Lọc hiển thị theo vai trò và cache theo "người xem":
            // - public: chỉ thấy job đã publish
            // - recruiter: thấy tất cả job đã publish + job draft do mình tạo
            // - admin: thấy tất cả
            // Cache key dạng: jobs:{viewerKey}:page:{page}:size:{pageSize}
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "public";
            var viewerKey = role == "admin" ? "admin" : role == "recruiter" ? $"recruiter:{userId}" : "public";

            var cacheKey = $"jobs:{viewerKey}:page:{page}:size:{pageSize}";
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

            var jobs = await _jobRepository.GetAllVisibleAsync(userId, role, page, pageSize);
            var jobDtos = jobs.Select(MapToJobDto).ToList();

            var totalCount = await _jobRepository.CountAllVisibleAsync(userId, role);

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
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
            // Quy tắc hiển thị theo vai trò khi xem chi tiết:
            // - admin: luôn xem được
            // - recruiter: xem được nếu job đã publish hoặc là job do mình tạo
            // - public/candidate: chỉ xem được nếu job đã publish
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound(new ApiResponse<Job>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc"
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "public";

            var visible = role switch
            {
                "admin" => true,
                "recruiter" => job.Status == "published" || job.CreatedBy == userId,
                _ => job.Status == "published"
            };

            if (!visible)
            {
                return NotFound(new ApiResponse<Job>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc"
                });
            }

            await _jobRepository.IncrementViewsAsync(id);

            job.Applicants ??= new List<Applicant>();

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

            // Invalidate cache: chỉ cần xóa cache theo scope của recruiter tạo job,
            // job ở trạng thái draft nên public/admin chưa bị ảnh hưởng
            await _cacheService.RemoveByPatternAsync($"jobs:recruiter:{userId}:*");

            return CreatedAtAction(nameof(GetJobById), new { id = created.Id }, new ApiResponse<Job>
            {
                Success = true,
                Message = "Tạo công việc thành công",
                Data = created
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "recruiter,admin")]
        public async Task<ActionResult<ApiResponse<Job>>> UpdateJob(string id, [FromBody] UpdateJobRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound(new ApiResponse<Job> { Success = false, Message = "Không tìm thấy công việc" });
            }

            // Recruiter chỉ được cập nhật job của chính mình
            if (role == "recruiter" && job.CreatedBy != userId)
            {
                return Forbid();
            }

            // Map nullable fields
            if (!string.IsNullOrEmpty(request.Title)) job.Title = request.Title;
            if (request.Salary != null)
            {
                job.Salary ??= new Salary();
                job.Salary.Min = request.Salary.Min;
                job.Salary.Max = request.Salary.Max;
                job.Salary.Currency = request.Salary.Currency;
                job.Salary.Type = request.Salary.Type;
            }
            if (!string.IsNullOrEmpty(request.Experience)) job.Experience = request.Experience;
            if (!string.IsNullOrEmpty(request.Education)) job.Education = request.Education;
            if (!string.IsNullOrEmpty(request.EmploymentType)) job.EmploymentType = request.EmploymentType;
            if (!string.IsNullOrEmpty(request.WorkMode)) job.WorkMode = request.WorkMode;
            if (request.Skills != null) job.Skills = request.Skills;
            if (request.Categories != null) job.Categories = request.Categories;
            if (!string.IsNullOrEmpty(request.JobDetails)) job.JobDetails = request.JobDetails;
            if (!string.IsNullOrEmpty(request.Requirements)) job.Requirements = request.Requirements;
            if (!string.IsNullOrEmpty(request.Benefits)) job.Benefits = request.Benefits;
            if (request.Workplace != null)
            {
                job.Workplace ??= new Workplace();
                job.Workplace.Address = request.Workplace.Address;
                job.Workplace.City = request.Workplace.City;
                job.Workplace.District = request.Workplace.District;
            }
            if (request.Vacancies.HasValue) job.Vacancies = request.Vacancies.Value;
            if (!string.IsNullOrEmpty(request.StartDate)) job.StartDate = request.StartDate;
            if (!string.IsNullOrEmpty(request.EndDate)) job.EndDate = request.EndDate;

            var updated = await _jobRepository.UpdateAsync(id, job);
            if (!updated)
            {
                return BadRequest(new ApiResponse<Job> { Success = false, Message = "Cập nhật công việc thất bại" });
            }

            // Invalidate cache trên tất cả scope vì nội dung job có thể ảnh hưởng tới public/admin
            await _cacheService.RemoveByPatternAsync("jobs:public:*");
            await _cacheService.RemoveByPatternAsync("jobs:admin:*");
            await _cacheService.RemoveByPatternAsync($"jobs:recruiter:{userId}:*");

            return Ok(new ApiResponse<Job>
            {
                Success = true,
                Message = "Cập nhật công việc thành công",
                Data = job
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "recruiter,admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteJob(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Không tìm thấy công việc" });
            }

            // Recruiter chỉ được xóa (soft-delete) job mình tạo
            if (role == "recruiter" && job.CreatedBy != userId)
            {
                return Forbid();
            }

            var deleted = await _jobRepository.DeleteAsync(id);
            if (!deleted)
            {
                return BadRequest(new ApiResponse<bool> { Success = false, Message = "Xóa công việc thất bại" });
            }

            // Invalidate cache trên tất cả scope vì trạng thái hiển thị thay đổi
            await _cacheService.RemoveByPatternAsync("jobs:public:*");
            await _cacheService.RemoveByPatternAsync("jobs:admin:*");
            await _cacheService.RemoveByPatternAsync($"jobs:recruiter:{userId}:*");

            return Ok(new ApiResponse<bool> { Success = true, Message = "Xóa công việc thành công", Data = true });
        }

        [HttpPatch("{id}/publish")]
        [Authorize(Roles = "recruiter,admin")]
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

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            // Recruiter chỉ được publish job mình tạo
            if (role == "recruiter" && job.CreatedBy != userId)
            {
                return Forbid();
            }

            job.Status = "published";
            job.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.UpdateAsync(id, job);

            // Invalidate cache: public có thể nhìn thấy job này sau khi publish
            await _cacheService.RemoveByPatternAsync("jobs:public:*");
            await _cacheService.RemoveByPatternAsync("jobs:admin:*");
            await _cacheService.RemoveByPatternAsync($"jobs:recruiter:{userId}:*");

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Đăng công việc thành công",
                Data = true
            });
        }

        [HttpPatch("{id}/unpublish")]
        [Authorize(Roles = "recruiter,admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UnpublishJob(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Không tìm thấy công việc" });
            }
            // Recruiter chỉ được gỡ đăng job của chính mình
            if (role == "recruiter" && job.CreatedBy != userId)
            {
                return Forbid();
            }

            job.Status = "draft";
            job.UpdatedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(id, job);

            // Invalidate cache: public không còn thấy job này
            await _cacheService.RemoveByPatternAsync("jobs:public:*");
            await _cacheService.RemoveByPatternAsync("jobs:admin:*");
            await _cacheService.RemoveByPatternAsync($"jobs:recruiter:{userId}:*");

            return Ok(new ApiResponse<bool> { Success = true, Message = "Gỡ đăng công việc thành công", Data = true });
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

            var totalCount = await _jobRepository.CountSearchJobsAsync(
                request.Keyword,
                request.City,
                request.Categories
            );

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = (int)totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
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

            var totalCount = await _jobRepository.CountJobsByCompanyAsync(companyId);

            var response = new PagedResponse<JobDto>
            {
                Items = jobDtos,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
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
