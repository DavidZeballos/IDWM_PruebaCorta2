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
chairs.MapGet("/{nombre}", GetChairByName);
chairs.MapPut("/{id}", UpdateChair);
chairs.MapPut("/{id}/stock", IncrementStock);
chairs.MapPost("/purchase", PurchaseChair);
chairs.MapDelete("/{id}", DeleteChair);

app.Run();

static IResult AddChair([FromBody] Chair chair, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Nombre == chair.Nombre).FirstOrDefault();
    if(existingChair != null)
    {
        return TypedResults.BadRequest("La silla ya existe!");
    }

    db.Chairs.Add(chair);
    db.SaveChanges();
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
        query = query.Where(c => c.Material == material);
    }
    if(color != string.Empty)
    {
        query = query.Where(c => c.Color == color);
    }

    List<Chair> list = query.ToList();
    return TypedResults.Ok(query.ToList());
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

static IResult UpdateChair([FromBody] UpdateChairDto chairUpdate, int id, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.NotFound("La silla a modificar no existe!");
    }

    existingChair.Nombre = chairUpdate.Nombre;
    existingChair.Tipo = chairUpdate.Tipo;
    existingChair.Material = chairUpdate.Material;
    existingChair.Color = chairUpdate.Color;
    existingChair.Altura = chairUpdate.Altura;
    existingChair.Anchura = chairUpdate.Anchura;
    existingChair.Profundidad = chairUpdate.Profundidad;
    existingChair.Precio = chairUpdate.Precio;

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

    existingChair.Stock += chairStock.Stock;

    db.Chairs.Entry(existingChair).State = EntityState.Modified;
    db.SaveChanges();
    return TypedResults.NoContent();
}

static IResult PurchaseChair([FromBody] PurchaseChairDto chairPurchase, DataContext db)
{
    var existingChair = db.Chairs.Where(c => c.Id == chairPurchase.Id).FirstOrDefault();
    if(existingChair == null)
    {
        return TypedResults.BadRequest("La silla no existe!");
    }

    var PrecioTotal = existingChair.Precio*chairPurchase.Cantidad;
    if(existingChair.Stock >= chairPurchase.Cantidad
        && PrecioTotal <= chairPurchase.TotalPagado)
    {
        existingChair.Stock -= chairPurchase.Cantidad;
        
        db.Chairs.Entry(existingChair).State = EntityState.Modified;
        db.SaveChanges();
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

