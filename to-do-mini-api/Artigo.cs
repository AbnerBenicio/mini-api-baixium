namespace to_do_mini_api
{
    public class Artigo
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public Usuario Autor { get; set; }
        public Tema Tema { get; set; }
    }
}
