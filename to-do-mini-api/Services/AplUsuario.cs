using to_do_mini_api.Model;
namespace to_do_mini_api.Services
{
    public class AplUsuario
    {
        public async Task<Usuario> CadastrarUsuario(Usuario user, BaixumDB db)
        {
            db.Usuarios.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

    }
}
