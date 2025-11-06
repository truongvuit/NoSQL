using Core.DTOs.Auth;
using Core.DTOs.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Refresh token thành công",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Đăng xuất thành công",
                Data = result
            });
        }
    }
}
