namespace to_do_mini_api.Model
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Administrador { get; set; }

    }
}


