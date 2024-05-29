using chairs_dotnet7_api;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("chairlist"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

var chairs = app.MapGroup("api/chair");
chairs.MapPost("/", AddChair);
chairs.MapGet("/", GetChairs);
chairs.MapGet("/{name}", GetChairByName);
chairs.MapPut("/{id}", UpdateChair);
chairs.MapPut("/{id}/stock", IncrementStock);
chairs.MapPost("/chair/purchase", PurchaseChair);
chairs.MapDelete("/{id}", DeleteChair);

app.Run();

static IResult AddChair([FromBody] Chair chair, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Nombre == chair.Nombre).FirstOrDefault();
    if(existingChair != null)
    {
        return TypedResults.BadRequest("La silla ya existe!");
    }

    return TypedResults.Created($"/chair/{chair.Id}", chair);
}

static IResult GetChairs(
    [FromQuery] string? tipo, 
    [FromQuery] string? material, 
    [FromQuery] string? color, DataContext db)
{
    IQueryable<Chair> query = db.Chairs;
    
    if(tipo != string.Empty)
    {
        query = query.Where(c => c.Tipo == tipo);
    }
    if(material != string.Empty)
    {
        query = query.Where(c => c.Tipo == material);
    }
    if(color != string.Empty)
    {
        query = query.Where(c => c.Tipo == color);
    }

    List<Chair> list = query.ToList();
    return TypedResults.Ok(list);
}

static IResult GetChairByName(string nombre, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Nombre == nombre).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.NotFound("La silla no existe!");
    }

    return TypedResults.Ok(existingChair);
}

static IResult UpdateChair([FromBody] UpdateChairDto chair, int id, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.NotFound("La silla a modificar no existe!");
    }

    existingChair.Nombre = chair.Nombre;
    existingChair.Tipo = chair.Tipo;
    existingChair.Material = chair.Material;
    existingChair.Color = chair.Color;
    existingChair.Altura = chair.Altura;
    existingChair.Anchura = chair.Anchura;
    existingChair.Profundidad = chair.Profundidad;
    existingChair.Precio = chair.Precio;

    db.Chairs.Entry(existingChair).State = EntityState.Modified;
    db.SaveChanges();
    return TypedResults.NoContent();
}

static IResult IncrementStock([FromBody] IncrementStockDto chairStock, int id, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.NotFound("No se ha podido aumentar el stock, la silla no existe!");
    }

    existingChair.Stock = existingChair.Stock + chairStock.Stock;

    db.Chairs.Entry(existingChair).State = EntityState.Modified;
    db.SaveChanges();
    return TypedResults.NoContent();
}

static IResult PurchaseChair([FromBody] PurchaseChairDto chair, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == chair.Id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.BadRequest("La silla no existe!");
    }

    var PrecioTotal = existingChair.Precio*chair.Cantidad;
    if(existingChair.Stock >= chair.Cantidad
        && PrecioTotal >= chair.TotalPagado)
    {
        existingChair.Stock = existingChair.Stock - chair.Cantidad;
        return TypedResults.NoContent();
    }

    return TypedResults.BadRequest("Ha ocurrido un error en la compra.");
}

static IResult DeleteChair(int id, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.NotFound("La silla no existe!");
    }
    db.Chairs.Remove(existingChair);
    db.SaveChanges();
    return TypedResults.NoContent();
}

