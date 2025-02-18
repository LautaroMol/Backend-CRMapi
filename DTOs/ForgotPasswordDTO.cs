using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
