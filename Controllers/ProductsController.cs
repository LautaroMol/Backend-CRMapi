using AutoMapper;
using CRMapi.DTOs;
using CRMapi.Models;
using CRMapi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public ProductsController(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            // Crear la carpeta 'uploads' si no existe
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Guardar las imágenes
            if (productDto.Image != null && productDto.Image.Count > 0)
            {
                product.Image = new List<string>();
                foreach (var file in productDto.Image)
                {
                    var filePath = Path.Combine(uploadsFolderPath, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    product.Image.Add($"/uploads/{file.FileName}");
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDTO productDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _mapper.Map(productDto, product);

            // Crear la carpeta 'uploads' si no existe
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Guardar las imágenes
            if (productDto.Image != null && productDto.Image.Count > 0)
            {
                product.Image = new List<string>();
                foreach (var file in productDto.Image)
                {
                    var filePath = Path.Combine(uploadsFolderPath, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    product.Image.Add($"/uploads/{file.FileName}");
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}