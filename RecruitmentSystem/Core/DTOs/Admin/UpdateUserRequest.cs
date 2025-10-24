
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Admin
{
    public class UpdateUserRequest
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public bool? IsVerified { get; set; }
        public string? FullName { get; set; }
    }
}
