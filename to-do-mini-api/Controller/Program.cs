using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using to_do_mini_api;
using to_do_mini_api.Model;
using to_do_mini_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Adicionando o Identity ao serviço
builder.Services.AddIdentity<Usuario, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<BaixumDB>();


// Adicionando o serviço de autorização
builder.Services.AddAuthorization();

//Criando base de dados
builder.Services.AddDbContext<BaixumDB>(options =>
{
    var connectionString = "Server=database-baixum.ccjww5dfpxwu.us-east-1.rds.amazonaws.com;Database=database_baixum;User ID=ArthurCremasco;Password=Bx!99-77;Trusted_Connection=False;TrustServerCertificate=True;";
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
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

// Adicionando o middleware do Identity
app.UseAuthentication();
app.UseAuthorization();

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


// Authentication Mapgroup
app.MapGroup("/identity").MapIdentityApi<IdentityUser>();
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

baixiumItems.MapPost("/register", async (Usuario model, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager) =>
{
    var user = new Usuario { UserName = model.Email, Email = model.Email, Nome = model.Nome, Administrador = model.Administrador };
    var result = await userManager.CreateAsync(user, model.PasswordHash);
    if (result.Succeeded)
    {
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.Ok(new { Message = "Usuário registrado com sucesso!" });
    }
    else
    {
        return Results.Problem("O registro falhou!");
    }
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
    AplTemas TemaService = new AplTemas();
    try
    {
        TemaService.PublicarArtigo(tema, db);
        return Results.Created($"/temas/{tema.Id}", tema);
        
    } 
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }


});

baixiumItems.MapGet("/temas", async (BaixumDB db) =>
{
    AplTemas TemaService = new AplTemas();
    return Results.Ok(await TemaService.BuscarTema(db));
});
    
    
await app.RunAsync();
