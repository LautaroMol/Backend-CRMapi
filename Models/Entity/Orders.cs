namespace CRMapi.Models.Entity
{
    public class Orders : EntityBase
    {
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public Clients Client { get; set; }
        public List <OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

    }
}
