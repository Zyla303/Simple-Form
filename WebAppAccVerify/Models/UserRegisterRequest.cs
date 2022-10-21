using System.ComponentModel.DataAnnotations;

namespace WebAppAccVerify.Models
{
    public class UserRegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(8, ErrorMessage = "Min length is 8"), MaxLength(36, ErrorMessage = "Max lenghth is 36")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password", ErrorMessage = "Passwords dont match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
