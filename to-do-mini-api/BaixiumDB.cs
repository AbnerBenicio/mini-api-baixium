using Microsoft.EntityFrameworkCore;
using to_do_mini_api.Model;

public class BaixumDB : DbContext
{
    public BaixumDB(DbContextOptions<BaixumDB> options)
        : base(options)
    {}

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Artigo> Artigos => Set<Artigo>();
    public DbSet<Tema> Temas => Set<Tema>();
}

