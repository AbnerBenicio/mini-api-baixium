namespace to_do_mini_api
{
    public class Artigo
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public bool Validado { get; set; }
        public virtual Usuario Autor { get; set; }
        public virtual Tema Tema { get; set; }
    }
}
