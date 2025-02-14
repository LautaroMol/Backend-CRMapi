using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMapi.Models.Entity
{
    public class OrderDetails : EntityBase
    {
        [ForeignKey("Order")]
        public int OrderNumber { get; set; }
        public Orders Order { get; set; }
        [ForeignKey("Product")]
        public int ProductCode { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
