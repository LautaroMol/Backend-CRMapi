﻿using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class PersonalDTO
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
        [Required(ErrorMessage = "El campo Email es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Email no puede tener mas de 50 caracteres")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Contraseña no puede tener mas de 50 caracteres")]
        public string Password { get; set; }
    }
}
