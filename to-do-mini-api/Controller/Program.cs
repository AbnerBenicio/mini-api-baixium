using Microsoft.EntityFrameworkCore;
using to_do_mini_api;
using to_do_mini_api.Model;
using to_do_mini_api.Services;


var builder = WebApplication.CreateBuilder(args);

//Criando base de dados
builder.Services.AddDbContext<BaixumDB>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
});

//Habilitando endpointa
builder.Services.AddEndpointsApiExplorer();

//Habilitando uso do swagger
builder.Services.AddSwaggerGen();

// Configure a política CORS para permitir origens específicas
builder.Services.AddCors(options =>
{
    var urlBaixium = Environment.GetEnvironmentVariable("URL_BAIXIUM");
    if (urlBaixium != null)
    {
        options.AddPolicy("AllowSpecificOrigins",
        builder => builder.WithOrigins(
            urlBaixium
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
    } else
    {
        options.AddPolicy("AllowSpecificOrigins",
        builder => builder.WithOrigins(
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
    }
});

var app = builder.Build();

// Permitindo acesso à API de qualquer origem
app.UseCors("AllowSpecificOrigins");


// Sempre use Swagger
app.UseSwagger();
app.UseSwaggerUI();

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
baixiumItems.MapPost("/login", async (LoginRequest loginRequest, BaixumDB db) =>
{
    // Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        // Utilizando service de login
        Usuario user = await UserService.Login(loginRequest.Password, loginRequest.Email, db);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        // Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint para recuperar senha
baixiumItems.MapPost("/recuperar-senha", async (RecSenha recupera, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de Recuperar senha
        await UserService.RecSenha(recupera.email, db);
        return Results.Ok("foi");
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

//Controller Artigos
baixiumItems.MapGet("/artigos/validos", async (BaixumDB db, Guid? autorId, Guid? temaId, int page, int limit) =>
{
    AplArtigo ArticleService = new AplArtigo();
    return Results.Ok(await ArticleService.BuscarArtigos(db, true, autorId, temaId, page, limit));
});

baixiumItems.MapGet("/artigos/invalidos", async (BaixumDB db, Guid? autorId, Guid? temaId, int page, int limit) =>
{
    AplArtigo ArticleService = new AplArtigo();
    return Results.Ok(await ArticleService.BuscarArtigos(db, false, autorId, temaId, page, limit));
});

baixiumItems.MapGet("/artigos/{id}", async (BaixumDB db, Guid id) =>
{
    AplArtigo ArticleService = new AplArtigo();

    try
    {
        var artigo = await ArticleService.BuscarArtigos(db, id);
        return Results.Ok(artigo);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound();
    }
});

baixiumItems.MapPost("/artigos", async (Artigo artigo, BaixumDB db) =>
{
    try
    {
        AplArtigo ArticleService = new AplArtigo();
        await ArticleService.PostarArtigo(artigo, db);
        return Results.Created($"/artigos/{artigo.Id}", artigo);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }

});


baixiumItems.MapPut("/artigos/{id}", async (Guid id, Artigo inputArtigo, BaixumDB db) =>
{
    AplArtigo ArticleService = new AplArtigo();

    try
    {
        await ArticleService.AtualizarArtigo(id, inputArtigo, db);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

baixiumItems.MapDelete("/artigos/{id}", async (Guid id, BaixumDB db) =>
{

    AplArtigo ArticleService = new AplArtigo();

    try
    {
        await ArticleService.DeletarArtigo(id, db);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

baixiumItems.MapPost("/temas", async (Tema tema, BaixumDB db) =>
{
    AplTemas temasService = new AplTemas();

    tema = await temasService.PublicarTema(tema, db);

    return Results.Created($"/temas/{tema.Id}", tema);
});

baixiumItems.MapGet("/temas", async (BaixumDB db) =>
{
    AplTemas temasService = new AplTemas();
    var temas = await temasService.BuscarTema(db);
    return temas;
});


await app.RunAsync();
