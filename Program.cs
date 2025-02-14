using AutoMapper;
using CRMapi.DTOs;
using CRMapi.Mappings;
using CRMapi.Models;
using CRMapi.Models.Entity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Configuraciones Entity Framework
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

//configuracion AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#region minimal api
//API routes
app.MapGet("/orders", async (Context db) =>
    await db.Orders.Include(o => o.OrderDetails).ToListAsync());

app.MapGet("/orders/{id}", async (int id, Context db) =>
    await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id)
        is Orders order
            ? Results.Ok(order)
            : Results.NotFound());

app.MapPost("/orders", async (OrdersDTO orderDto, Context db, IMapper mapper) =>
{
    var order = mapper.Map<Orders>(orderDto);

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // Descontar el stock de los productos
    foreach (var detail in order.OrderDetails)
    {
        var product = await db.Products.FindAsync(detail.ProductCode);
        if (product != null)
        {
            product.Stock -= detail.Quantity;
        }
    }
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapPut("/orders/{id}", async (int id, OrdersDTO orderDto, Context db, IMapper mapper) =>
{
    var order = await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

    if (order is null) return Results.NotFound();

    mapper.Map(orderDto, order);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/orders/{id}", async (int id, Context db) =>
{
    var order = await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

    if (order is null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

//product

#endregion

app.MapControllers();

app.UseStaticFiles();

app.Run();
