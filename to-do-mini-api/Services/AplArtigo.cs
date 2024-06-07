using Microsoft.EntityFrameworkCore;
using to_do_mini_api.Model;

namespace to_do_mini_api.Services
{
    public class AplArtigo
    {
        public async Task<Artigo> PostarArtigo(Artigo artigo, BaixumDB db)
        {
            // Busca o usuário e o tema no banco de dados
            Usuario usuarioExistente = await db.Usuarios.FindAsync(artigo.Autor.Id);
            Tema temaExistente = await db.Temas.FindAsync(artigo.Tema.Id);

            if (usuarioExistente == null || temaExistente == null)
            {
                // Se o usuário ou o tema não existirem, retorna um erro
                throw new ArgumentException("Você deve escolher um tema válido.");
            }
            else
            {
                // Se o usuário e o tema existirem, associa o artigo a eles
                artigo.Autor = usuarioExistente;
                artigo.Tema = temaExistente;
            }

            // Adiciona o artigo ao banco de dados
            db.Artigos.Add(artigo);
            await db.SaveChangesAsync();

            return artigo;
        }

        public async Task<Artigo> BuscarArtigos(BaixumDB db, Guid? id)
        {
            Artigo artigo = await db.Artigos
                .Include(a => a.Autor) // Carregamento antecipado do Autor
                .Include(a => a.Tema) // Carregamento antecipado do Tema
                .FirstOrDefaultAsync(a => a.Id == id) ?? new Artigo();

            return artigo;
        }

        public async Task<List<Artigo>> BuscarArtigos(BaixumDB db, Guid? autorId, Guid? temaId)
        {
            IQueryable<Artigo> query = db.Artigos
                .Include(a => a.Autor)
                .Include(a => a.Tema);

            if (autorId != null)
            {
                query = query.Where(a => a.Autor.Id == autorId);
            }

            if (temaId != null)
            {
                query = query.Where(a => a.Tema.Id == temaId);
            }

            return await query.ToListAsync();
        }

        public async Task AtualizarArtigo(Guid id, Artigo inputArtigo, BaixumDB db)
        {
            var artigo = await db.Artigos.FindAsync(id);

            if (artigo != null)
            {
                artigo.Titulo = inputArtigo.Titulo;
                artigo.Conteudo = inputArtigo.Conteudo;
                artigo.Validado = inputArtigo.Validado;
                artigo.Autor = inputArtigo.Autor;
                artigo.Tema = inputArtigo.Tema;

                await db.SaveChangesAsync();
            }
            else
            {
                //Lançando erro, caso artigo não exista
                throw new ArgumentException("Artigo não existe");
            }
        }

        public async Task DeletarArtigo(Guid id, BaixumDB db)
        {
            if (!(await db.Artigos.FindAsync(id) is Artigo artigo))
            {
                //Lançando erro, caso artigo não exista
                throw new ArgumentException("Artigo não existe");
            }
            else
            {
                db.Artigos.Remove(artigo);
                await db.SaveChangesAsync();
            }
        }

    }
}
