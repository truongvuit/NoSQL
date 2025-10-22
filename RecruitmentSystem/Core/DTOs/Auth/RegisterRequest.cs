using Core.DTOs.Common;
using RecruitmentSystem.Core.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}

// File: RecruitmentSystem.Core/DTOs/User/UserDto.cs
namespace RecruitmentSystem.Core.DTOs.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public UserProfileDto Profile { get; set; }
        public string CompanyId { get; set; }
        public CompanyDto Company { get; set; }
    }

    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public AddressDto Address { get; set; }
        public string Bio { get; set; }
    }

    public class AddressDto
    {
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public AddressDto Address { get; set; }
        public string Bio { get; set; }
    }
}
