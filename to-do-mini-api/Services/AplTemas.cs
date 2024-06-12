using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using to_do_mini_api.Model;

namespace to_do_mini_api.Services
{
    public class AplTemas
    {
        public async Task<Tema> PublicarTema(Tema tema, BaixumDB db)
        {
            if (tema != null)
            {
                db.Temas.Add(tema);
                await db.SaveChangesAsync();
                return tema;
            }
            else
            {
                throw new ArgumentException("Tema não existe");
            }
        }

        public async Task<List<Tema>> BuscarTema(BaixumDB db)
        {
             return await db.Temas.ToListAsync();
        }
    }
}
