using Microsoft.AspNetCore.Identity;


namespace to_do_mini_api.Model
{
    public class Usuario : IdentityUser<Guid>
    {
        public string Nome { get; set; }
        public bool Administrador { get; set; }
    }
}


