﻿using Microsoft.EntityFrameworkCore;
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

            if (!string.IsNullOrWhiteSpace(artigo.Titulo) && !string.IsNullOrWhiteSpace(artigo.Conteudo))
            {
                // Adiciona o artigo ao banco de dados
                db.Artigos.Add(artigo);
                await db.SaveChangesAsync();

                return artigo;
            } else
            {
                throw new ArgumentException("Todos os campos devem ser preenchidos.");
            }

            
        }

        public async Task<Artigo> BuscarArtigos(BaixumDB db, Guid? id)
        {
            Artigo artigo = await db.Artigos
                .Include(a => a.Autor) // Carregamento antecipado do Autor
                .Include(a => a.Tema) // Carregamento antecipado do Tema
                .FirstOrDefaultAsync(a => a.Id == id) ?? new Artigo();

            return artigo;
        }

        public async Task<List<Artigo>> BuscarArtigos(BaixumDB db, bool validado, Guid? autorId, Guid? temaId, int page, int limit)
        {

            IQueryable<Artigo> query = db.Artigos
                .Include(a => a.Autor)
                .Include(a => a.Tema);

            if (autorId != null && autorId != Guid.Empty)
            {
                query = query.Where(a => a.Autor.Id == autorId);
            }

            if (temaId != null && temaId != Guid.Empty)
            {
                query = query.Where(a => a.Tema.Id == temaId);
            }

            query = query.Where(a => a.Validado == validado);

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }


        public async Task AtualizarArtigo(Guid id, Artigo inputArtigo, BaixumDB db)
        {
            var artigo = await db.Artigos.FindAsync(id);

            if (artigo != null)
            {
                if (inputArtigo.Tema.Id != Guid.Empty && !string.IsNullOrEmpty(inputArtigo.Titulo) && !string.IsNullOrEmpty(inputArtigo.Conteudo))
                {
                    Usuario usuarioExistente = await db.Usuarios.FindAsync(inputArtigo.Autor.Id);
                    Tema temaExistente = await db.Temas.FindAsync(inputArtigo.Tema.Id);

                    artigo.Titulo = inputArtigo.Titulo;
                    artigo.Conteudo = inputArtigo.Conteudo;
                    artigo.Validado = inputArtigo.Validado;
                    artigo.Autor = usuarioExistente;
                    artigo.Tema = temaExistente;

                    await db.SaveChangesAsync();
                } else
                {
                    throw new ArgumentException("Informações inválidas");
                }
                

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
