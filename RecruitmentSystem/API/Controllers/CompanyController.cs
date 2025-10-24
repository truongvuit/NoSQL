using Core.DTOs.Common;
using Core.DTOs.Company;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private const string PendingCompaniesVersionKey = "pending_companies_version";

        public CompanyController(IUserRepository userRepository, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        [HttpPost("register")]
        [Authorize(Roles = "candidate")]
        public async Task<ActionResult<ApiResponse<CompanyRegistrationResponse>>> RegisterCompany([FromBody] CompanyRegistrationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<CompanyRegistrationResponse>
                    {
                        Success = false,
                        Message = "Người dùng không được xác thực"
                    });
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<CompanyRegistrationResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                if (user.Company != null)
                {
                    return BadRequest(new ApiResponse<CompanyRegistrationResponse>
                    {
                        Success = false,
                        Message = "Bạn đã đăng ký công ty trước đó"
                    });
                }

                var company = new Company
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = request.Name,
                    Website = request.Website,
                    Email = request.Email,
                    Phone = request.Phone,
                    EmployeeSize = request.EmployeeSize,
                    BusinessField = request.BusinessField,
                    TaxCode = request.TaxCode,
                    FoundedYear = request.FoundedYear ?? 0,
                    Introduction = request.Introduction,
                    Vision = request.Vision,
                    Mission = request.Mission,
                    CoreValues = request.CoreValues,
                    LogoUrl = request.LogoUrl,
                    Location = request.Location != null ? new CompanyLocation
                    {
                        Address = request.Location.Address,
                        City = request.Location.City,
                        District = request.Location.District,
                    } : null,
                    Verified = false,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                user.Company = company;
                var result = await _userRepository.UpdateAsync(userId, user);

                if (!result)
                {
                    return BadRequest(new ApiResponse<CompanyRegistrationResponse>
                    {
                        Success = false,
                        Message = "Đăng ký công ty thất bại"
                    });
                }
                
                await _cacheService.IncrementAsync(PendingCompaniesVersionKey);

                return Ok(new ApiResponse<CompanyRegistrationResponse>
                {
                    Success = true,
                    Message = "Đăng ký công ty thành công, vui lòng chờ admin duyệt",
                    Data = new CompanyRegistrationResponse
                    {
                        Id = company.Id,
                        Name = company.Name,
                        Verified = company.Verified,
                        Status = "pending"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CompanyRegistrationResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<PagedResponse<PendingCompanyDto>>>> GetPendingCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                long cacheVersion = await _cacheService.IncrementAsync(PendingCompaniesVersionKey, 0);
                var cacheKey = $"pending_companies_v{cacheVersion}_p{page}_s{pageSize}";

                var cachedResponse = await _cacheService.GetAsync<ApiResponse<PagedResponse<PendingCompanyDto>>>(cacheKey);
                if (cachedResponse != null)
                {
                    return Ok(cachedResponse);
                }

                var usersWithPendingCompanies = await _userRepository.GetPendingCompaniesAsync(page, pageSize);
                
                var pendingCompaniesDto = new List<PendingCompanyDto>();
                foreach (var user in usersWithPendingCompanies)
                {
                    if (user.Company != null)
                    {
                        pendingCompaniesDto.Add(new PendingCompanyDto
                        {
                            Id = user.Company.Id,
                            Name = user.Company.Name,
                            Website = user.Company.Website,
                            Email = user.Company.Email,
                            Phone = user.Company.Phone,
                            BusinessField = user.Company.BusinessField,
                            TaxCode = user.Company.TaxCode,
                            LogoUrl = user.Company.LogoUrl,
                            CreatedAt = user.Company.CreatedAt,
                            RequestedBy = new UserCompanyDto
                            {
                                Id = user.Id,
                                Email = user.Email,
                                FullName = user.Profile?.FullName,
                                Phone = user.Phone
                            }
                        });
                    }
                }

                var totalCount = await _userRepository.CountPendingCompaniesAsync();
                
                var response = new ApiResponse<PagedResponse<PendingCompanyDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách công ty chờ duyệt thành công",
                    Data = new PagedResponse<PendingCompanyDto>
                    {
                        Items = pendingCompaniesDto,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = (int)totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                };
                
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<PagedResponse<PendingCompanyDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("verify")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyCompany([FromBody] CompanyVerificationRequest request)
        {
            try
            {
                var result = await _userRepository.ApproveCompanyAsync(request.CompanyId, request.Approve, request.RejectionReason);
                if (!result)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = request.Approve ? "Duyệt công ty thất bại" : "Từ chối công ty thất bại"
                    });
                }

                await _cacheService.IncrementAsync(PendingCompaniesVersionKey);

                await _cacheService.RemoveAsync($"company:{request.CompanyId}");

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = request.Approve ? "Duyệt công ty thành công" : "Từ chối công ty thành công",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}