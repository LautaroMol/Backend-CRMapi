namespace CRMapi.Models.Entity
{
    public class OrderDetails : EntityBase
    {
        public int OrderNumber { get; set; }
        public Orders Order { get; set; }

        public int ProductCode { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
