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

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CompanyDetailDto>>>> GetAllCompanies(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? verified = null)
        {
            try
            {
                var companies = await _userRepository.GetAllCompaniesAsync(page, pageSize, verified);
                var totalCount = await _userRepository.CountCompaniesAsync(verified);

                var companiesDto = companies.Select(c => new CompanyDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Website = c.Website,
                    Email = c.Email,
                    Phone = c.Phone,
                    EmployeeSize = c.EmployeeSize,
                    BusinessField = c.BusinessField,
                    TaxCode = c.TaxCode,
                    FoundedYear = c.FoundedYear,
                    Introduction = c.Introduction,
                    Vision = c.Vision,
                    Mission = c.Mission,
                    CoreValues = c.CoreValues,
                    Location = c.Location != null ? new Core.DTOs.Company.CompanyLocationDto
                    {
                        Address = c.Location.Address,
                        City = c.Location.City,
                        District = c.Location.District,
                        Country = c.Location.Country
                    } : null,
                    Tier = c.Tier,
                    Verified = c.Verified,
                    VerifiedAt = c.VerifiedAt,
                    IsActive = c.IsActive,
                    LogoUrl = c.LogoUrl,
                    CoverUrl = c.CoverUrl,
                    Images = c.Images,
                    Benefits = c.Benefits,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

                var response = new ApiResponse<PagedResponse<CompanyDetailDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách công ty thành công",
                    Data = new PagedResponse<CompanyDetailDto>
                    {
                        Items = companiesDto,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = (int)totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<PagedResponse<CompanyDetailDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CompanyDetailDto>>> GetCompanyById(string id)
        {
            try
            {
                var company = await _userRepository.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound(new ApiResponse<CompanyDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy công ty"
                    });
                }

                var companyDto = new CompanyDetailDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Slug = company.Slug,
                    Website = company.Website,
                    Email = company.Email,
                    Phone = company.Phone,
                    EmployeeSize = company.EmployeeSize,
                    BusinessField = company.BusinessField,
                    TaxCode = company.TaxCode,
                    FoundedYear = company.FoundedYear,
                    Introduction = company.Introduction,
                    Vision = company.Vision,
                    Mission = company.Mission,
                    CoreValues = company.CoreValues,
                    Location = company.Location != null ? new Core.DTOs.Company.CompanyLocationDto
                    {
                        Address = company.Location.Address,
                        City = company.Location.City,
                        District = company.Location.District,
                        Country = company.Location.Country
                    } : null,
                    Tier = company.Tier,
                    Verified = company.Verified,
                    VerifiedAt = company.VerifiedAt,
                    IsActive = company.IsActive,
                    LogoUrl = company.LogoUrl,
                    CoverUrl = company.CoverUrl,
                    Images = company.Images,
                    Benefits = company.Benefits,
                    CreatedAt = company.CreatedAt,
                    UpdatedAt = company.UpdatedAt
                };

                return Ok(new ApiResponse<CompanyDetailDto>
                {
                    Success = true,
                    Message = "Lấy thông tin công ty thành công",
                    Data = companyDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CompanyDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,recruiter")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCompany(string id, [FromBody] UpdateCompanyRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var company = await _userRepository.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy công ty"
                    });
                }

                // Recruiter chỉ có thể cập nhật công ty của mình
                if (userRole == "recruiter")
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user.Company == null || user.Company.Id != id)
                    {
                        return Forbid();
                    }
                }

                // Update company fields
                if (!string.IsNullOrEmpty(request.Name)) company.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Website)) company.Website = request.Website;
                if (!string.IsNullOrEmpty(request.Email)) company.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Phone)) company.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.EmployeeSize)) company.EmployeeSize = request.EmployeeSize;
                if (!string.IsNullOrEmpty(request.BusinessField)) company.BusinessField = request.BusinessField;
                if (!string.IsNullOrEmpty(request.TaxCode)) company.TaxCode = request.TaxCode;
                if (request.FoundedYear.HasValue) company.FoundedYear = request.FoundedYear.Value;
                if (!string.IsNullOrEmpty(request.Introduction)) company.Introduction = request.Introduction;
                if (!string.IsNullOrEmpty(request.Vision)) company.Vision = request.Vision;
                if (!string.IsNullOrEmpty(request.Mission)) company.Mission = request.Mission;
                if (request.CoreValues != null) company.CoreValues = request.CoreValues;
                if (!string.IsNullOrEmpty(request.LogoUrl)) company.LogoUrl = request.LogoUrl;
                if (!string.IsNullOrEmpty(request.CoverUrl)) company.CoverUrl = request.CoverUrl;
                if (request.Images != null) company.Images = request.Images;
                if (request.Benefits != null) company.Benefits = request.Benefits;

                if (request.Location != null)
                {
                    company.Location = new Core.Models.CompanyLocation
                    {
                        Address = request.Location.Address,
                        City = request.Location.City,
                        District = request.Location.District,
                        Country = request.Location.Country ?? "Việt Nam"
                    };
                }

                var result = await _userRepository.UpdateCompanyAsync(id, company);
                if (!result)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cập nhật công ty thất bại"
                    });
                }

                await _cacheService.RemoveAsync($"company:{id}");

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Cập nhật công ty thành công",
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCompany(string id)
        {
            try
            {
                var company = await _userRepository.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy công ty"
                    });
                }

                var result = await _userRepository.DeleteCompanyAsync(id);
                if (!result)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Xóa công ty thất bại"
                    });
                }

                await _cacheService.RemoveAsync($"company:{id}");

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Xóa công ty thành công",
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