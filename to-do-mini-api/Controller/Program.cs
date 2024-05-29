using Microsoft.EntityFrameworkCore;
using to_do_mini_api.Model;
using to_do_mini_api.Services;


var builder = WebApplication.CreateBuilder(args);

//Criando base de dados
builder.Services.AddDbContext<BaixumDB>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BloggingDatabase"), options => options.EnableRetryOnFailure());
});

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

//Criando grupo
var baixiumItems = app.MapGroup("/baixium");

//Encaminhando para endpoints da api
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }

    await next();
});

//Endpoint Get (para todos os usuários)
baixiumItems.MapGet("/usuarios", async (BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    //Buscando usuários
    return Results.Ok(await UserService.BuscarUsuarios(db));
});

//Endpoint Get (para usuário específico)
baixiumItems.MapGet("/usuarios/{id}", async (Guid id, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de buscar usuário
        var usuario = await UserService.BuscarUsuarios(id, db);
        return Results.Ok(usuario);
    }
    catch (ArgumentException ex)
    {
        //Retornando erro
        return Results.NotFound(new { message = ex.Message });
    }
});

//Endpoint Post de Usuários
baixiumItems.MapPost("/usuarios", async (Usuario user, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de cadastrar usuários
        user = await UserService.CadastrarUsuario(user, db);
        return Results.Created($"/usuarios/{user.Id}", user);
    } catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint para login
baixiumItems.MapPost("/login", async (string password, string email, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de login
        Usuario user = await UserService.Login(password, email, db);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint para recuperar senha
baixiumItems.MapPost("/recuperar-senha", async (string email, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de Recuperar senha
        Usuario user = await UserService.RecSenha(email, db);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint Put de Usuários
baixiumItems.MapPut("/usuarios/{id}", async (Guid id, Usuario inputUser, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();

    try {
        //Utilizando service de atualizar usuário
        await UserService.AtualizarUsuario(id, inputUser, db);
        return Results.NoContent();
    } catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint Delete de Usuários
baixiumItems.MapDelete("/usuarios/{id}", async (Guid id, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    //Tentando deletar usuário
    try
    {
        //Utilizando service de exclusão
        await UserService.DeletarUsuario(id, db);
        return Results.NoContent();
    } catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }

});

baixiumItems.MapGet("/artigos", async (BaixumDB db, Guid? autorId, Guid? temaId) =>
{
    IQueryable<Artigo> query = db.Artigos
        .Include(a => a.Autor)
        .Include(a => a.Tema);

    if (autorId != null)
    {
        query = query.Where(a => a.Autor.Id == autorId);
    }

    if (temaId != null)
    {
        query = query.Where(a => a.Tema.Id == temaId);
    }

    return await query.ToListAsync();
});

baixiumItems.MapGet("/artigos/{id}", async (Guid id, BaixumDB db) =>
{
    Artigo artigo = await db.Artigos
        .Include(a => a.Autor) // Carregamento antecipado do Autor
        .Include(a => a.Tema) // Carregamento antecipado do Tema
        .FirstOrDefaultAsync(a => a.Id == id) ?? new Artigo();

    return artigo.Id == default ? Results.NotFound() : Results.Ok(artigo);
});

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


baixiumItems.MapPut("/artigos/{id}", async (Guid id, Artigo inputArtigo, BaixumDB db) =>
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

baixiumItems.MapDelete("/{id}", async (Guid id, BaixumDB db) =>
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
