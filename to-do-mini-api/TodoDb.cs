using Microsoft.EntityFrameworkCore;
using to_do_mini_api;

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Usuario> Todos => Set<Usuario>();
}
