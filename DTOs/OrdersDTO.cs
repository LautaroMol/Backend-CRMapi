using CRMapi.Models.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class OrdersDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderNumber { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public string CustomerDni { get; set; }
        [Required]
        public List<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    }
}
