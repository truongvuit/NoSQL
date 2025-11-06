using Core.DTOs.Admin;
using Core.DTOs.Common;
using Core.DTOs.User;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using AutoMapper;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, ICacheService cacheService, IMapper mapper)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _mapper = mapper;
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

            var userDto = _mapper.Map<UserDto>(user);
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
                Data = _mapper.Map<UserDto>(user)
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
                Data = _mapper.Map<UserDto>(user)
            });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserSummaryDto>>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userRepository.GetAllAsync(page, pageSize);
            var totalCount = await _userRepository.CountAsync();

            var userDtos = _mapper.Map<List<UserSummaryDto>>(users);

            var response = new ApiResponse<PagedResponse<UserSummaryDto>>
            {
                Success = true,
                Message = "Lấy danh sách người dùng thành công",
                Data = new PagedResponse<UserSummaryDto>
                {
                    Items = userDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = (int)totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, [FromBody] Core.DTOs.Admin.UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto> { Success = false, Message = "Không tìm thấy người dùng" });
            }

            // Update properties from request
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;
            user.Role = request.Role ?? user.Role;
            user.IsVerified = request.IsVerified ?? user.IsVerified;
            if (user.Profile != null)
            {
                user.Profile.FullName = request.FullName ?? user.Profile.FullName;
            }

            var result = await _userRepository.UpdateAsync(id, user);
            if (!result)
            {
                return BadRequest(new ApiResponse<UserDto> { Success = false, Message = "Cập nhật người dùng thất bại" });
            }
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"user:{id}:profile");


            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Cập nhật người dùng thành công",
                Data = _mapper.Map<UserDto>(user)
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
        {
            var result = await _userRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Không tìm thấy người dùng hoặc lỗi khi xoá" });
            }
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"user:{id}:profile");

            return Ok(new ApiResponse<bool> { Success = true, Message = "Xoá người dùng thành công", Data = true });
        }
    }
}