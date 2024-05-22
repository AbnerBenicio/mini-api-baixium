using Microsoft.EntityFrameworkCore;
using to_do_mini_api;


var builder = WebApplication.CreateBuilder(args);

//Criando base de dados
builder.Services.AddDbContext<BaixumDB>(opt => opt
    .UseLazyLoadingProxies() // Habilita o uso de proxies
    .UseInMemoryDatabase("TodoList"));

//Habilitando endpointa
builder.Services.AddEndpointsApiExplorer();

//Habilitando uso do swagger
builder.Services.AddSwaggerGen();

//Habilitando acesso à api
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin",
        builder => builder.WithOrigins("http://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod());
});

var app = builder.Build();

//Permitindo acesso à api
app.UseCors(policy =>
    policy.WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var baixiumItems = app.MapGroup("/baixium");

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }

    await next();
});

baixiumItems.MapGet("/usuarios", async (BaixumDB db) =>
    await db.Usuarios.ToListAsync());

baixiumItems.MapGet("/usuarios/{id}", async (string id, BaixumDB db) =>
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

baixiumItems.MapPut("/usuarios/{id}", async (string id, Usuario inputUser, BaixumDB db) =>
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

baixiumItems.MapDelete("/usuarios/{id}", async (string id, BaixumDB db) =>
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

baixiumItems.MapGet("/artigos/{id}", async (Guid id, BaixumDB db) =>
    await db.Artigos.FindAsync(id)
        is Artigo artigo
            ? Results.Ok(artigo)
            : Results.NotFound());

baixiumItems.MapPost("/artigos", async (Artigo artigo, BaixumDB db) =>
{
    // Busca o usuário e o tema no banco de dados
    Usuario usuarioExistente = await db.Usuarios.FindAsync(artigo.Autor.Id);
    Tema temaExistente = await db.Temas.FindAsync(artigo.Tema.Id);

    if (usuarioExistente == null || temaExistente == null)
    {
        // Se o usuário ou o tema não existirem, retorna um erro
        return Results.BadRequest("Usuário ou Tema não existem");
    }
    else
    {
        // Se o usuário e o tema existirem, associa o artigo a eles
        artigo.Autor = usuarioExistente;
        artigo.Tema = temaExistente;
    }

    // Adiciona o artigo ao banco de dados
    db.Artigos.Add(artigo);
    await db.SaveChangesAsync();

    return Results.Created($"/artigos/{artigo.Id}", artigo);
});


baixiumItems.MapPut("/artigos/{id}", async (string id, Artigo inputArtigo, BaixumDB db) =>
{
    var artigo = await db.Artigos.FindAsync(id);

    if (artigo is null) return Results.NotFound();

    artigo.Titulo = inputArtigo.Titulo;
    artigo.Conteudo = inputArtigo.Conteudo;
    artigo.Validado = inputArtigo.Validado;
    artigo.Autor = inputArtigo.Autor;
    artigo.Tema = inputArtigo.Tema;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

baixiumItems.MapDelete("/{id}", async (string id, BaixumDB db) =>
{
    if (await db.Artigos.FindAsync(id) is Artigo artigo)
    {
        db.Artigos.Remove(artigo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

baixiumItems.MapPost("/temas", async (Tema tema, BaixumDB db) =>
{
    db.Temas.Add(tema);
    await db.SaveChangesAsync();

    return Results.Created($"/temas/{tema.Id}", tema);
});

baixiumItems.MapGet("/temas", async (BaixumDB db) =>
    await db.Temas.ToListAsync());

app.Run();
