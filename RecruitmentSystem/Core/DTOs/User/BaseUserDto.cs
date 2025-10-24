namespace Core.DTOs.User
{
    public class BaseUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
    }
}
