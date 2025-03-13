using CRMapi.DTOs;
using CRMapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using CRMapi.Models.Entity;
using Microsoft.AspNetCore.Identity.Data;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class PersonalController : ControllerBase
{
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Personal> _userManager;

    public PersonalController(Context context, IMapper mapper, IConfiguration configuration, UserManager<Personal> userManager)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _userManager = userManager;
    }

    /// Register
    [HttpPost("register")]
    public async Task<IActionResult> Register(PersonalDTO personalDTO)
    {
        if (await _context.Personals.AnyAsync(p => p.NormalizedEmail == personalDTO.Email.ToUpper()))
        {
            return BadRequest("El email ya está registrado.");
        }

        var personal = _mapper.Map<Personal>(personalDTO);
        personal.PasswordHash = BCrypt.Net.BCrypt.HashPassword(personalDTO.Password);
        personal.NormalizedEmail = personalDTO.Email.ToUpper(); // Normalizar email

        _context.Personals.Add(personal);
        await _context.SaveChangesAsync();

        return Ok("Usuario registrado correctamente.");
    }

    /// Login return a JWT

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            return Unauthorized("Credenciales incorrectas.");
        }

        // Obtener roles del usuario
        var roles = await _userManager.GetRolesAsync(user);
        string userRole = roles.FirstOrDefault() ?? "SinRol"; // Si no tiene rol, devuelve "SinRol"

        // Generar token JWT
        var token = GenerateJwtToken(user, userRole);

        return Ok(new { Token = token, Role = userRole });
    }

    /// Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Personal>>> GetAll()
    {
        return await _context.Personals.ToListAsync();
    }

    /// <summary>
    /// users by dni
    /// </summary>
    [HttpGet("{dni}")]
    public async Task<ActionResult<Personal>> GetByDni(string dni)
    {
        var personal = await _context.Personals.FirstOrDefaultAsync(p => p.Dni == dni);
        if (personal == null) return NotFound("Usuario no encontrado.");
        return Ok(personal);
    }

    /// update
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string id, PersonalDTO personalDTO)
    {
        var personal = await _context.Personals.FindAsync(id);
        if (personal == null) return NotFound("Usuario no encontrado.");

        if (await _context.Personals.AnyAsync(p => p.NormalizedEmail == personalDTO.Email.ToUpper() && p.Id != id))
        {
            return BadRequest("El email ya está registrado por otro usuario.");
        }

        personal.Name = personalDTO.Name;
        personal.LastName = personalDTO.LastName;
        personal.Dni = personalDTO.Dni;
        personal.Email = personalDTO.Email;
        personal.NormalizedEmail = personalDTO.Email.ToUpper();
        personal.PasswordHash = BCrypt.Net.BCrypt.HashPassword(personalDTO.Password);

        await _context.SaveChangesAsync();
        return Ok("Usuario actualizado correctamente.");
    }

    /// Delete users by id
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var personal = await _context.Personals.FindAsync(id);
        if (personal == null) return NotFound("Usuario no encontrado.");

        _context.Personals.Remove(personal);
        await _context.SaveChangesAsync();
        return Ok("Usuario eliminado correctamente.");
    }

    /// JWT generation
    private string GenerateJwtToken(Personal user, string role)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
