using Microsoft.EntityFrameworkCore;
using to_do_mini_api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BaixumDB>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var baixiumItems = app.MapGroup("/baixium");

baixiumItems.MapGet("/usuarios", async (BaixumDB db) =>
    await db.Usuarios.ToListAsync());

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }

    await next();
});

baixiumItems.MapGet("/usuarios/{id}", async (int id, BaixumDB db) =>
    await db.Usuarios.FindAsync(id)
        is Usuario user
            ? Results.Ok(user)
            : Results.NotFound());

baixiumItems.MapPost("/usuarios", async (Usuario user, BaixumDB db) =>
{
    db.Usuarios.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/usuarios/{user.Id}", user);
});

baixiumItems.MapPut("/usuarios/{id}", async (int id, Usuario inputUser, BaixumDB db) =>
{
    var user = await db.Usuarios.FindAsync(id);

    if (user is null) return Results.NotFound();

    user.Nome = inputUser.Nome;
    user.Email = inputUser.Email;
    user.Password = inputUser.Password;
    user.Adm = inputUser.Adm;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

baixiumItems.MapDelete("/usuarios/{id}", async (int id, BaixumDB db) =>
{
    if (await db.Usuarios.FindAsync(id) is Usuario user)
    {
        db.Usuarios.Remove(user);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

baixiumItems.MapGet("/artigos", async (BaixumDB db) =>
    await db.Artigos.ToListAsync());

baixiumItems.MapGet("/artigos/{id}", async (int id, BaixumDB db) =>
    await db.Artigos.FindAsync(id)
        is Artigo artigo
            ? Results.Ok(artigo)
            : Results.NotFound());

baixiumItems.MapPost("/artigos", async (Artigo artigo, BaixumDB db) =>
{
    db.Artigos.Add(artigo);
    await db.SaveChangesAsync();

    return Results.Created($"/artigos/{artigo.Id}", artigo);
});

baixiumItems.MapPut("/artigos/{id}", async (int id, Artigo inputArtigo, BaixumDB db) =>
{
    var artigo = await db.Artigos.FindAsync(id);

    if (artigo is null) return Results.NotFound();

    artigo.Titulo = inputArtigo.Titulo;
    artigo.Conteudo = inputArtigo.Conteudo;
    artigo.Autor = inputArtigo.Autor;
    artigo.Tema = inputArtigo.Tema;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

baixiumItems.MapDelete("/{id}", async (int id, BaixumDB db) =>
{
    if (await db.Artigos.FindAsync(id) is Artigo artigo)
    {
        db.Artigos.Remove(artigo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

baixiumItems.MapGet("/temas", async (BaixumDB db) =>
    await db.Temas.ToListAsync());

app.Run();
