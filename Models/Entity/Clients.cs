using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRMapi.Models.Entity
{
    [Index(nameof(Dni),Name ="Dni_UQ", IsUnique = true)]
    public class Clients : EntityBase
    {
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Nombre no puede tener mas de 50 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El campo Apellido es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Apellido no puede tener mas de 50 caracteres")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "El Dni es requerido")]
        [MaxLength(8, ErrorMessage = "El campo Dni no puede tener mas de 8 caracteres")]
        public string Dni { get; set; }
        [Required(ErrorMessage = "El Email es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Email no puede tener mas de 50 caracteres")]
        public string Email { get; set; }
        public string? Phone { get; set; }
        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        public string password { get; set; }
    }
}
