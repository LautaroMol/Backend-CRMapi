using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
