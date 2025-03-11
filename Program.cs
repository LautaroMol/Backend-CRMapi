using AutoMapper;
using CRMapi.DTOs;
using CRMapi.Mappings;
using CRMapi.Models;
using CRMapi.Models.Entity;
using CRMapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TuProyecto;


var builder = WebApplication.CreateBuilder(args);

async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
{
    Console.WriteLine("Ejecutando SeedData...");  // 👈 Agregado para verificar ejecución

    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // Crear roles si no existen
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Rol {role} creado.");
        }
    }

    // Crear usuario Admin si no existe
    string adminEmail = "admin@example.com";
    string adminPassword = "Admin123!";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
        }
        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Usuario Admin creado.");
        }
        else
        {
            Console.WriteLine("Error al crear usuario Admin:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
}



//Configuraciones Entity Framework
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

//configuracion AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings"));
builder.Services.AddTransient<IMessage, Message>();

builder.Services.AddIdentity<Personal, IdentityRole>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();


var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Asegúrate de que la autenticación esté configurada
app.UseAuthorization();

app.MapControllers();
//seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}


#region orders
//API routes
app.MapGet("/orders", async (Context db) =>
    await db.Orders.Include(o => o.OrderDetails).ToListAsync());

app.MapGet("/orders/{orderNumber}", async (int orderNumber, Context db) =>
    await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber)
        is Orders order
            ? Results.Ok(order)
            : Results.NotFound());

app.MapPost("/orders", async (OrdersDTO orderDto, Context db, IMapper mapper) =>
{
    var client = await db.Clients.FirstOrDefaultAsync(c => c.Dni == orderDto.ClientDni);
    if (client == null)
    {
        return Results.BadRequest("Cliente no encontrado.");
    }

    var order = mapper.Map<Orders>(orderDto);

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    if (order.Status == "Entregado" || order.Status == "Reservado")
    {
        // Descontar el stock de los productos
        var orderDetails = await db.OrderDetails.Where(od => od.OrderNumber == order.OrderNumber).ToListAsync();
        foreach (var detail in orderDetails)
        {
            var product = await db.Products.FindAsync(detail.ProductCode);
            if (product != null)
            {
                product.Stock -= detail.Quantity;
            }
        }
        await db.SaveChangesAsync();
    }

    return Results.Created($"/orders/{order.OrderNumber}", order);
});


app.MapPut("/orders/{ordernumber}", async (int ordernumber, OrdersDTO orderDto, Context db, IMapper mapper) =>
{
    var order = await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderNumber == ordernumber);

    if (order is null) return Results.NotFound();

    mapper.Map(orderDto, order);

    await db.SaveChangesAsync();
    if (order.Status == "Entregado" || order.Status == "Reservado")
    {
        // Descontar el stock de los productos
        var orderDetails = await db.OrderDetails.Where(od => od.OrderNumber == order.OrderNumber).ToListAsync();
        foreach (var detail in orderDetails)
        {
            var product = await db.Products.FindAsync(detail.ProductCode);
            if (product != null)
            {
                product.Stock -= detail.Quantity;
            }
        }
        await db.SaveChangesAsync();
    }

    return Results.Ok();
});

app.MapDelete("/orders/{ordernumber}", async (int ordernumber, Context db) =>
{
    var order = await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderNumber == ordernumber);

    if (order is null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

#region Clients
app.MapPost("/clients", async (ClientsDTO clientDto, Context db, IMessage messageService) =>
{
    if (await db.Clients.AnyAsync(c => c.Email == clientDto.Email))
    {
        return Results.BadRequest("El email ya está registrado.");
    }

    var client = new Clients
    {
        Name = clientDto.Name,
        LastName = clientDto.LastName,
        Dni = clientDto.Dni,
        Email = clientDto.Email,
        Phone = clientDto.Phone,
        password = BCrypt.Net.BCrypt.HashPassword(clientDto.password), //  Hasheo de contraseña
    };

    db.Clients.Add(client);
    await db.SaveChangesAsync();

    // Enviar correo de confirmación
    var subject = "Confirmación de registro";
    var body = $"Hola {client.Name},\n\nGracias por registrarte en nuestro sistema. Tu cuenta ha sido creada exitosamente.";
    messageService.SendEmail(subject, body, client.Email);

    return Results.Created($"/clients/{client.Id}", client);
});

app.MapGet("/clients", async (Context db) =>
    await db.Clients.Where(c => c.Activo).ToListAsync());

app.MapGet("/clients/{dni:int}", async (string dni, Context db) =>
{
    var client = await db.Clients.FirstOrDefaultAsync(p => p.Dni == dni);
    return client is not null ? Results.Ok(client) : Results.NotFound();
});

app.MapPut("/clients/{id:int}", async (int id, ClientsDTO clientDto, Context db) =>
{
    var client = await db.Clients.FindAsync(id);
    if (client is null) return Results.NotFound();

    client.Name = clientDto.Name;
    client.LastName = clientDto.LastName;
    client.Dni = clientDto.Dni;
    client.Email = clientDto.Email;
    client.Phone = clientDto.Phone;
    client.password = BCrypt.Net.BCrypt.HashPassword(clientDto.password); // Debe ser hasheada

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPost("/sendEmail", (SendEmailRequest request) =>
{
    var service = app.Services.GetRequiredService<IMessage>();

    service!.SendEmail(request.Subject, request.Body, request.To);
});

app.MapPost("/auth/login", async (LoginDTO loginDto, Context db) =>
{
    var user = await db.Clients.FirstOrDefaultAsync(c => c.Email == loginDto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.password))
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]); // Usa la clave fuerte

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        }),
        Expires = DateTime.UtcNow.AddHours(2), // Duración del token
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = jwt });
});

app.MapPost("/auth/forgot-password", async (ForgotPasswordDTO forgotPasswordDto, Context db, IMessage messageService) =>
{
    var user = await db.Clients.FirstOrDefaultAsync(c => c.Email == forgotPasswordDto.Email);
    if (user == null)
    {
        return Results.NotFound("Usuario no encontrado.");
    }

    // Generar un token de recuperación de contraseña
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, user.Email) }),
        Expires = DateTime.UtcNow.AddHours(1), // El token expira en 1 hora
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var resetToken = tokenHandler.WriteToken(token);

    // Enviar correo con el token de recuperación
    var resetLink = $"https://localhost:7236/reset-password?token={resetToken}"; //aqui se deberia colocar el dominio usado
    var subject = "Recuperación de contraseña";
    var body = $"Hola {user.Name},\n\nPara restablecer tu contraseña, por favor haz clic en el siguiente enlace: {resetLink}\n\nEste enlace expirará en 1 hora.";
    messageService.SendEmail(subject, body, user.Email);

    return Results.Ok("Correo de recuperación enviado.");
});

app.MapPost("/auth/reset-password", async (ResetPasswordDTO resetPasswordDto, Context db) =>
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);

    try
    {
        var principal = tokenHandler.ValidateToken(resetPasswordDto.Token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null)
        {
            return Results.BadRequest("Token inválido.");
        }

        var user = await db.Clients.FirstOrDefaultAsync(c => c.Email == email);
        if (user == null)
        {
            return Results.NotFound("Usuario no encontrado.");
        }

        // Actualizar la contraseña del usuario
        user.password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
        await db.SaveChangesAsync();

        return Results.Ok("Contraseña restablecida exitosamente.");
    }
    catch
    {
        return Results.BadRequest("Token inválido.");
    }
});

app.MapGet("/clientes-protegidos", [Authorize] async (Context db) =>
    await db.Clients.Where(c => c.Activo).ToListAsync());

app.MapDelete("/clients/{id:int}", async (int id, Context db) =>
{
    var client = await db.Clients.FindAsync(id);
    if (client is null) return Results.NotFound();

    db.Clients.Remove(client);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

#region OrdersDetails
app.MapGet("/orderDetails", async (Context db) =>
    await db.OrderDetails.ToListAsync());


app.MapGet("/orderDetails/{orderNumber:int}", async (int orderNumber, Context db) =>
{
    await db.OrderDetails.Where(od => od.OrderNumber == orderNumber).ToListAsync();
});

app.MapPost("/orderDetails", async (OrderDetailsDTO ordDetailDTO, Context db, IMapper mapper) =>
{
    var orderDetail = mapper.Map<OrderDetails>(ordDetailDTO);
    db.OrderDetails.Add(orderDetail);
    await db.SaveChangesAsync();
});

app.MapPut("/orderDetails/{id}", async (int id,OrderDetailsDTO ordDetailDTO, Context db, IMapper mapper) =>
{
    var orderDetail = await db.OrderDetails.FindAsync(id);
    if (orderDetail is null) return Results.NotFound();
    mapper.Map(ordDetailDTO, orderDetail);

    await db.SaveChangesAsync();
    return Results.Ok();

});

app.MapDelete("/orderDetails/{id}", async (int id, Context db) =>
{
    var orderDetail = await db.OrderDetails.FindAsync(id);
    if (orderDetail is null) return Results.NotFound();
    db.OrderDetails.Remove(orderDetail);
    await db.SaveChangesAsync();
    return Results.Ok();
});

#endregion

#region Personal

app.MapPost("/personal", async(PersonalDTO personalDTO, Context db, IMapper mapper
    ,IMessage messageService)=>
{
    if (await db.Personals.AnyAsync(c => c.Email == personalDTO.Email))
    {
        return Results.BadRequest("El email ya está registrado.");
    }
    var Personal = mapper.Map<Personal>(personalDTO);
    Personal.PasswordHash = BCrypt.Net.BCrypt.HashPassword(personalDTO.Password);
    db.Personals.Add(Personal);
    await db.SaveChangesAsync();

    var subject = "Confirmación de registro";
    var body = $"Hola {personalDTO.Name},\n\n Su usuario ha sido dado de alta en el sistema.";
    messageService.SendEmail(subject, body, personalDTO.Email);

    return Results.Ok();
});

app.MapGet("/personal", async (Context db) =>
    await db.Personals.ToListAsync());

app.MapGet("/personal/{dni:int}", async (string dni, Context db) =>
{
    Personal personal = await db.Personals.FirstOrDefaultAsync(p => p.Dni == dni);
    return personal is not null ? Results.Ok(personal) : Results.NotFound();
});

app.MapPut("/personal/{dni:int}", async (int id,PersonalDTO personalDTO,Context db) =>
{
    if (await db.Personals.AnyAsync(c => c.Email == personalDTO.Email && c.Dni != personalDTO.Dni))
    {
        return Results.BadRequest("El email ya está registrado.");
    }
    var Personal = await db.Personals.FindAsync(id);
    if (Personal is null) return Results.NotFound();
    Personal.Name = personalDTO.Name;
    Personal.LastName = personalDTO.LastName;
    Personal.Dni = personalDTO.Dni;
    Personal.Email = personalDTO.Email;
    Personal.PasswordHash = BCrypt.Net.BCrypt.HashPassword(personalDTO.Password);
    await db.SaveChangesAsync();

    return Results.Ok();

});

app.MapDelete("/personal/{dni:int}", async (int dni, Context db) =>
{
    var Personal = await db.Personals.FindAsync(dni);
    if (Personal is null) return Results.NotFound();

    db.Personals.Remove(Personal);
    await db.SaveChangesAsync();

    return Results.Ok();

});

#endregion


app.MapControllers();

app.UseStaticFiles();

app.Run();