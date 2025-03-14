using CRMapi.DTOs;
using CRMapi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[Route("api/personal")]
[ApiController]
public class PersonalController : ControllerBase
{
    private readonly UserManager<Personal> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public PersonalController(UserManager<Personal> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // User Registry
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] PersonalDTO model)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email.ToUpper());
        if (existingUser != null)
        {
            return BadRequest("El email ya está registrado.");
        }

        var user = new Personal
        {
            Name = model.Name,
            LastName = model.LastName,
            Dni = model.Dni,
            UserName = model.Name.ToUpper(),
            Email = model.Email.ToUpper(),
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Si el email es del dueño, asignarle "Administrator", sino "User" por defecto
        string role = model.Email == "laumol158@gmail.com" ? "Administrator" : "User";
        await _userManager.AddToRoleAsync(user, role);

        return Ok(new { message = "Usuario registrado exitosamente", role });
    }


    // Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized("Credenciales incorrectas.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles
            }
        });
    }

    // GetALl Users
    [HttpGet("usuarios")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        return Ok(users);
    }

    // Get users By DNI
    [HttpGet("{dni}")]
    public async Task<IActionResult> GetUserByDni(string dni)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Dni == dni);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        return Ok(user);
    }

    // Update Users
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] PersonalDTO model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        user.Name = model.Name;
        user.LastName = model.LastName;
        user.Dni = model.Dni;
        user.Email = model.Email.ToUpper();
        user.UserName = model.Name.ToUpper();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Usuario actualizado exitosamente" });
    }


    //rol update
    [HttpPut("rol-update/{id}")]
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> ChangeUserRole(string id, [FromBody] string newRole)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!await _roleManager.RoleExistsAsync(newRole))
            return BadRequest("El rol especificado no existe.");

        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRoleAsync(user, newRole);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"El usuario ahora tiene el rol {newRole}" });
    }


    // Delete Users
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Usuario eliminado exitosamente" });
    }

    // Generate JWT Token
    private string GenerateJwtToken(Personal user, IList<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}