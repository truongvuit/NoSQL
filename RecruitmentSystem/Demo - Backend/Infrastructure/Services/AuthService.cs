using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Core.DTOs.Auth;
using Core.DTOs.User;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Configuration;
using BCrypt.Net;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            ICacheService cacheService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác");
            }

            if (!user.IsActive || user.IsBanned)
            {
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa");
            }

            await _userRepository.UpdateLastLoginAsync(user.Id);

            var accessToken = GenerateAccessToken(user.Id, user.Email, user.Role);
            var refreshToken = GenerateRefreshToken();

            await _cacheService.SetAsync($"refresh_token:{refreshToken}", user.Id,
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays));

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = MapToUserDto(user)
            };
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email đã được sử dụng");
            }

            var user = new User
            {
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 10),
                Role = "candidate",
                IsVerified = false,
                Profile = new UserProfile
                {
                    FullName = request.FullName
                },
                Statistics = new UserStatistics()
            };

            await _userRepository.CreateAsync(user);

            var accessToken = GenerateAccessToken(user.Id, user.Email, user.Role);
            var refreshToken = GenerateRefreshToken();

            await _cacheService.SetAsync($"refresh_token:{refreshToken}", user.Id,
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays));

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = MapToUserDto(user)
            };
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            var userId = await _cacheService.GetAsync<string>($"refresh_token:{refreshToken}");

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Người dùng không tồn tại");
            }

            await _cacheService.RemoveAsync($"refresh_token:{refreshToken}");

            var newAccessToken = GenerateAccessToken(user.Id, user.Email, user.Role);
            var newRefreshToken = GenerateRefreshToken();

            await _cacheService.SetAsync($"refresh_token:{newRefreshToken}", user.Id,
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays));

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = MapToUserDto(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            return await _cacheService.RemoveAsync($"refresh_token:{refreshToken}");
        }

        public string GenerateAccessToken(string userId, string email, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private UserDto MapToUserDto(User user)
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
                    Bio = user.Profile.Bio
                } : null
            };
        }
    }
}
