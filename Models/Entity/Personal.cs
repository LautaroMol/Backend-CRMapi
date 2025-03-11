using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMapi.Models.Entity
{
    [Index(nameof(Dni), Name = "Dni_UQ", IsUnique = true)]
    public class Personal : IdentityUser
    {
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Nombre no puede tener mas de 50 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El Apellido es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Apellido no puede tener mas de 50 caracteres")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "El campo DNI es requerido")]
        [MaxLength(50, ErrorMessage = "El campo DNI no puede tener mas de 50 caracteres")]
        public string Dni { get; set; }
        
    }
}
