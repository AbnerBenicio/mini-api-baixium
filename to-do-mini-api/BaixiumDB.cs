using Microsoft.EntityFrameworkCore;
using to_do_mini_api;

public class BaixumDB : DbContext
{
    public BaixumDB(DbContextOptions<BaixumDB> options)
        : base(options)
    {
        // Habilita o uso de proxies para suporte ao carregamento lento
        this.ChangeTracker.LazyLoadingEnabled = true;
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Artigo> Artigos => Set<Artigo>();
    public DbSet<Tema> Temas => Set<Tema>();
}

