namespace to_do_mini_api;
using Microsoft.EntityFrameworkCore;

class BaixumDB : DbContext
{
    public BaixumDB(DbContextOptions<BaixumDB> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Artigo> Artigos => Set<Artigo>();
    public DbSet<Tema> Temas => Set<Tema>();
}
