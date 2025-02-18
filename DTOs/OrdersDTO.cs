using CRMapi.Models.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class OrdersDTO
    {
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public string ClientDni { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
