using CRMapi.Models.Entity;

namespace CRMapi.DTOs
{
    public class OrderDetailsDTO
    {
        public int OrderNumber { get; set; }
        public int ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}
