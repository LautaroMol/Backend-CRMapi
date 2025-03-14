﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace CRMapi.Models.Entity
{
    [Index(nameof(Code), Name = "Code_UQ", IsUnique = true)]
    public class Product : EntityBase
    {
        [Required(ErrorMessage = "El Código es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Código no puede tener mas de 50 caracteres")]
        public string Code { get; set; }
        [Required(ErrorMessage = "El Nombre es requerido")]
        [MaxLength(50, ErrorMessage = "El campo Nombre no puede tener mas de 50 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "La Descripción es requerida")]
        [MaxLength(180, ErrorMessage = "El campo Descripción no puede tener mas de 50 caracteres")]
        public string Description { get; set; }
        [Required(ErrorMessage = "El Precio es requerido")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "El Stock es requerido")]
        public int Stock { get; set; }
        public List<string> Image { get; set; } = new List<string>();
    }
}
