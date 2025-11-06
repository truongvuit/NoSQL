using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.DTOs.Common;
using Core.Models;

namespace API.Controllers
{
    /// <summary>
    /// Test Controller - Chỉ sử dụng cho development và testing
    /// KHÔNG sử dụng trong production
    /// </summary>
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IUserRepository userRepository, 
            IJobRepository jobRepository,
            ILogger<TestController> logger)
        {
            _userRepository = userRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách users (không cần auth)
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<object>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Test endpoint: Getting users");
                var users = await _userRepository.GetAllAsync(page, pageSize);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test endpoint - Lấy danh sách users thành công",
                    Data = new
                    {
                        users = users.Select(u => new
                        {
                            id = u.Id,
                            email = u.Email,
                            role = u.Role,
                            isVerified = u.IsVerified,
                            fullName = u.Profile?.FullName,
                            createdAt = u.CreatedAt
                        }),
                        totalCount = users.Count,
                        page = page,
                        pageSize = pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint GetUsers");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách users",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Lấy danh sách jobs (không cần auth)
        /// </summary>
        [HttpGet("jobs")]
        public async Task<ActionResult<ApiResponse<object>>> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Test endpoint: Getting jobs");
                var jobs = await _jobRepository.GetAllAsync(page, pageSize);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test endpoint - Lấy danh sách jobs thành công",
                    Data = new
                    {
                        jobs = jobs.Select(j => new
                        {
                            id = j.Id,
                            title = j.Title,
                            companyName = j.CompanySnapshot?.Name,
                            status = j.Status,
                            views = j.Views,
                            applicationCount = j.ApplicationCount,
                            createdAt = j.CreatedAt
                        }),
                        totalCount = jobs.Count,
                        page = page,
                        pageSize = pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint GetJobs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách jobs",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết job (không cần auth)
        /// </summary>
        [HttpGet("jobs/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetJobById(string id)
        {
            try
            {
                _logger.LogInformation("Test endpoint: Getting job by ID: {JobId}", id);
                var job = await _jobRepository.GetByIdAsync(id);
                
                if (job == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy job",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test endpoint - Lấy thông tin job thành công",
                    Data = new
                    {
                        id = job.Id,
                        title = job.Title,
                        companySnapshot = job.CompanySnapshot,
                        salary = job.Salary,
                        experience = job.Experience,
                        education = job.Education,
                        employmentType = job.EmploymentType,
                        workMode = job.WorkMode,
                        skills = job.Skills,
                        categories = job.Categories,
                        status = job.Status,
                        views = job.Views,
                        applicationCount = job.ApplicationCount,
                        createdAt = job.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint GetJobById");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi lấy thông tin job",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Tạo test user (không cần auth)
        /// </summary>
        [HttpPost("create-test-user")]
        public async Task<ActionResult<ApiResponse<object>>> CreateTestUser([FromBody] CreateTestUserRequest request)
        {
            try
            {
                _logger.LogInformation("Test endpoint: Creating test user");
                
                var testUser = new User
                {
                    Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                    Email = request.Email ?? "test@example.com",
                    Phone = request.Phone ?? "0123456789",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = request.Role ?? "candidate",
                    IsVerified = true,
                    Profile = new UserProfile
                    {
                        FullName = request.FullName ?? "Test User",
                        Avatar = "https://example.com/avatar.jpg",
                        Gender = "Male",
                        DateOfBirth = "1990-01-01",
                        Bio = "Test user created by test endpoint"
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Note: Trong thực tế, bạn sẽ gọi _userRepository.CreateAsync(testUser)
                // Nhưng để tránh tạo dữ liệu test thật, chúng ta chỉ trả về thông tin

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test user created successfully (not saved to database)",
                    Data = new
                    {
                        id = testUser.Id,
                        email = testUser.Email,
                        role = testUser.Role,
                        fullName = testUser.Profile.FullName,
                        createdAt = testUser.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint CreateTestUser");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi tạo test user",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Test endpoint để kiểm tra authentication
        /// </summary>
        [HttpGet("auth-info")]
        public ActionResult<ApiResponse<object>> GetAuthInfo()
        {
            var user = User;
            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Test endpoint - Thông tin authentication",
                Data = new
                {
                    isAuthenticated = user.Identity?.IsAuthenticated ?? false,
                    authenticationType = user.Identity?.AuthenticationType,
                    name = user.Identity?.Name,
                    claims = claims,
                    roles = user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
                }
            });
        }

        /// <summary>
        /// Test endpoint để kiểm tra database connection
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<ApiResponse<object>>> HealthCheck()
        {
            try
            {
                // Test database connection bằng cách lấy 1 user
                var users = await _userRepository.GetAllAsync(1, 1);
                var jobs = await _jobRepository.GetAllAsync(1, 1);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Health check passed",
                    Data = new
                    {
                        timestamp = DateTime.UtcNow,
                        database = "Connected",
                        usersCount = users.Count,
                        jobsCount = jobs.Count,
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Health check failed",
                    Data = new
                    {
                        timestamp = DateTime.UtcNow,
                        error = ex.Message,
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                    }
                });
            }
        }

        /// <summary>
        /// Test endpoint để clear cache (nếu có)
        /// </summary>
        [HttpPost("clear-cache")]
        public ActionResult<ApiResponse<object>> ClearCache()
        {
            // Trong thực tế, bạn sẽ gọi cache service để clear cache
            // Ở đây chỉ là demo

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Cache cleared successfully (demo)",
                Data = new
                {
                    timestamp = DateTime.UtcNow,
                    action = "clear_cache"
                }
            });
        }
    }

    /// <summary>
    /// Request model cho tạo test user
    /// </summary>
    public class CreateTestUserRequest
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}
