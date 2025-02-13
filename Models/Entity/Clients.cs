namespace CRMapi.Models.Entity
{
    public class Clients : EntityBase
    {
        public string Name { get; set; }

        public string LastName { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string password { get; set; }
    }
}
