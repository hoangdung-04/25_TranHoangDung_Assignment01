using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class AccountCreateDto
    {
        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Account Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email address")]
        [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
        public string AccountEmail { get; set; } = null!;

        [Required(ErrorMessage = "Account Role is required")]
        [Range(1, 2, ErrorMessage = "Role must be 1 (Staff) or 2 (Lecturer)")]
        public int AccountRole { get; set; } = 1;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(70, MinimumLength = 1, ErrorMessage = "Password must be at least 1 character")]
        public string AccountPassword { get; set; } = null!;
    }

    public class AccountUpdateDto
    {
        [Required(ErrorMessage = "AccountID is required")]
        public short AccountID { get; set; }

        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Account Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email address")]
        [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
        public string AccountEmail { get; set; } = null!;

        [Required(ErrorMessage = "Account Role is required")]
        [Range(1, 2, ErrorMessage = "Role must be 1 (Staff) or 2 (Lecturer)")]
        public int AccountRole { get; set; }

        [StringLength(70, ErrorMessage = "Password cannot exceed 70 characters")]
        public string? AccountPassword { get; set; }
    }
}
