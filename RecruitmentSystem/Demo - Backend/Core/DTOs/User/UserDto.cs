using Core.DTOs.Common;

namespace Core.DTOs.User
{
    public class UserDto : BaseUserDto
    {
        public string Phone { get; set; }
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