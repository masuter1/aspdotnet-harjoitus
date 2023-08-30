using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BooksDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/todoitems", async (BooksDb db) =>
    await db.Books.ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, BooksDb db) =>
    await db.Books.FindAsync(id)
        is Books todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Books todo, BooksDb db) =>
{
    db.Books.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/books/{todo.Id}", todo);
});

app.MapPut("/books/{id}", async (int id, Books inputTodo, BooksDb db) =>
{
    var todo = await db.Books.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, BooksDb db) =>
{
    if (await db.Books.FindAsync(id) is Books todo)
    {
        db.Books.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFIles")),
    RequestPath = "/StaticFIles",
    EnableDefaultFiles = true
});

app.Run();

public class Books
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

class BooksDb : DbContext
{
    public BooksDb(DbContextOptions<BooksDb> options)
    : base(options) { }

    public DbSet<Books> Books => Set<Books>();
}