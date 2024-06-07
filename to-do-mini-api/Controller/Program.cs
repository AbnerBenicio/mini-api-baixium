using Microsoft.EntityFrameworkCore;
using to_do_mini_api;
using to_do_mini_api.Model;
using to_do_mini_api.Services;


var builder = WebApplication.CreateBuilder(args);

//Criando base de dados
builder.Services.AddDbContext<BaixumDB>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("STRING_CONNECTION");
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
});

//Habilitando endpointa
builder.Services.AddEndpointsApiExplorer();

//Habilitando uso do swagger
builder.Services.AddSwaggerGen();

//Habilitando acesso � api
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin",
        builder => builder.WithOrigins("http://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod());
});

var app = builder.Build();

//Permitindo acesso � api
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

//Endpoint Get (para todos os usu�rios)
baixiumItems.MapGet("/usuarios", async (BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    //Buscando usu�rios
    return Results.Ok(await UserService.BuscarUsuarios(db));
});

//Endpoint Get (para usu�rio espec�fico)
baixiumItems.MapGet("/usuarios/{id}", async (Guid id, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de buscar usu�rio
        var usuario = await UserService.BuscarUsuarios(id, db);
        return Results.Ok(usuario);
    }
    catch (ArgumentException ex)
    {
        //Retornando erro
        return Results.NotFound(new { message = ex.Message });
    }
});

//Endpoint Post de Usu�rios
baixiumItems.MapPost("/usuarios", async (Usuario user, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    try
    {
        //Utilizando service de cadastrar usu�rios
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

//Endpoint Put de Usu�rios
baixiumItems.MapPut("/usuarios/{id}", async (Guid id, Usuario inputUser, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();

    try {
        //Utilizando service de atualizar usu�rio
        await UserService.AtualizarUsuario(id, inputUser, db);
        return Results.NoContent();
    } catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }
});

//Endpoint Delete de Usu�rios
baixiumItems.MapDelete("/usuarios/{id}", async (Guid id, BaixumDB db) =>
{
    //Instanciando Classe de service
    AplUsuario UserService = new AplUsuario();
    //Tentando deletar usu�rio
    try
    {
        //Utilizando service de exclus�o
        await UserService.DeletarUsuario(id, db);
        return Results.NoContent();
    } catch (Exception ex)
    {
        //Retornando erro
        return Results.Problem(ex.Message);
    }

});



//Controller Artigos
baixiumItems.MapGet("/artigos", async (BaixumDB db, Guid? autorId, Guid? temaId) =>
{
    AplArtigo ArticleService = new AplArtigo();
    return Results.Ok(await ArticleService.BuscarArtigos(db, autorId, temaId));
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

baixiumItems.MapDelete("/{id}", async (Guid id, BaixumDB db) =>
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
    db.Temas.Add(tema);
    await db.SaveChangesAsync();

    return Results.Created($"/temas/{tema.Id}", tema);
});

baixiumItems.MapGet("/temas", async (BaixumDB db) =>
    await db.Temas.ToListAsync());

app.Run();
