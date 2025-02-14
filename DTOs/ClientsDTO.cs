using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class ClientsDTO
    {
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Nombre no puede tener mas de 50 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El campo Apellido es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Apellido no puede tener mas de 50 caracteres")]
        public string LastName { get; set; }
        public string? Dni { get; set; }
        [Required(ErrorMessage = "El Email es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Email no puede tener mas de 50 caracteres")]
        public string Email { get; set; }

        public string? Phone { get; set; }
        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Contraseña no puede tener mas de 50 caracteres")]
        public string password { get; set; }
    }
}
