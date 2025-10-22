using Core.DTOs.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.Core.DTOs.User;
using System.Security.Claims;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;

        public UsersController(IUserRepository userRepository, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cacheKey = $"user:{userId}:profile";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);

            if (cachedUser != null)
            {
                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Lấy thông tin thành công (cached)",
                    Data = cachedUser
                });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng"
                });
            }

            var userDto = MapToUserDto(user);
            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(10));

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Lấy thông tin thành công",
                Data = userDto
            });
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng"
                });
            }

            user.Phone = request.Phone ?? user.Phone;
            user.Profile.FullName = request.FullName ?? user.Profile.FullName;
            user.Profile.Gender = request.Gender ?? user.Profile.Gender;
            user.Profile.DateOfBirth = request.DateOfBirth ?? user.Profile.DateOfBirth;
            user.Profile.Bio = request.Bio ?? user.Profile.Bio;

            if (request.Address != null)
            {
                user.Profile.Address = new Core.Models.Address
                {
                    City = request.Address.City,
                    District = request.Address.District,
                    Street = request.Address.Street
                };
            }

            await _userRepository.UpdateAsync(userId, user);
            await _cacheService.RemoveAsync($"user:{userId}:profile");

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Cập nhật thông tin thành công",
                Data = MapToUserDto(user)
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng"
                });
            }

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Data = MapToUserDto(user)
            });
        }

        private UserDto MapToUserDto(Core.Models.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsVerified = user.IsVerified,
                Profile = user.Profile != null ? new UserProfileDto
                {
                    FullName = user.Profile.FullName,
                    Avatar = user.Profile.Avatar,
                    Gender = user.Profile.Gender,
                    DateOfBirth = user.Profile.DateOfBirth,
                    Bio = user.Profile.Bio,
                    Address = user.Profile.Address != null ? new AddressDto
                    {
                        City = user.Profile.Address.City,
                        District = user.Profile.Address.District,
                        Street = user.Profile.Address.Street
                    } : null
                } : null,
                Company = user.Company != null ? new CompanyDto
                {
                    Id = user.Company.Id,
                    Name = user.Company.Name,
                    LogoUrl = user.Company.LogoUrl,
                    Website = user.Company.Website,
                    Email = user.Company.Email,
                    Phone = user.Company.Phone,
                    EmployeeSize = user.Company.EmployeeSize,
                    BusinessField = user.Company.BusinessField,
                    Introduction = user.Company.Introduction,
                    Verified = user.Company.Verified
                } : null
            };
        }
    }
}
