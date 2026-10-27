using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public short AccountID { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public int Role { get; set; } // 0: Admin, 1: Staff, 2: Lecturer
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserProfileDto
    {
        public short AccountID { get; set; }

        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? AccountName { get; set; }

        public string? AccountEmail { get; set; }
        public int? AccountRole { get; set; }

        [StringLength(70, MinimumLength = 1, ErrorMessage = "Password cannot exceed 70 characters")]
        public string? AccountPassword { get; set; }
    }
}
