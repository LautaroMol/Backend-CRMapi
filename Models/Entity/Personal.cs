namespace CRMapi.Models.Entity
{
    public class Personal : EntityBase
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
