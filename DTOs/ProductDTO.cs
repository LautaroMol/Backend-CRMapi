using CRMapi.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace CRMapi.DTOs
{
    public class ProductDTO
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

        public List<IFormFile> Image { get; set; } = new List<IFormFile>();

    }
}
