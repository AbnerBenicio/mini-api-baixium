using Microsoft.EntityFrameworkCore;
using to_do_mini_api.Model;
namespace to_do_mini_api.Services
{
    public class AplUsuario
    {
        //Método para cadastro de usuário
        public async Task<Usuario> CadastrarUsuario(Usuario user, BaixumDB db)
        {
            //Verificando se todos os campos estão preenchidos
            if (!string.IsNullOrWhiteSpace(user.Nome) && !string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.Email))
            {
                //Verificando se usuário já existe
                Usuario existingUser = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    //Lançando erro, caso exista
                    throw new ArgumentException("Já existe um usuário cadastrado com esse email.");
                }

                //Criptografando a senha do usuário
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                //Adicionando usuário ao banco de dados
                db.Usuarios.Add(user);
                await db.SaveChangesAsync();

                //Retornando usuário
                return user;
            }
            else
            {
                //Lançando erro, caso tenha campos vazios
                throw new ArgumentException("Nome, Senha e Email não podem ser vazios.");
            }
        }


        //Método para exibir usuários
        public async Task<List<Usuario>> BuscarUsuarios(BaixumDB db)
        {
            //Retornando usuários
            return await db.Usuarios.ToListAsync();
        }

        //Método para exibir usuário específico
        public async Task<Usuario> BuscarUsuarios(Guid id, BaixumDB db)
        {
            //Buscando usuário
            Usuario user = await db.Usuarios.FindAsync(id);
            if (user != null) {
                //Retornando usuário
                return user;
            } else
            {
                //Lançando erro, caso usuário não exista
                throw new ArgumentException("Usuário não existe");
            }
             
        }

        //Método para atualizar usuário
        public async Task AtualizarUsuario (Guid id, Usuario inputUser, BaixumDB db)
        {
            //Buscando usuário
            Usuario user = await this.BuscarUsuarios(id, db);
            //Verificando se campos estão preenchidos
            if (!string.IsNullOrWhiteSpace(inputUser.Nome) && !string.IsNullOrWhiteSpace(inputUser.Password) && !string.IsNullOrWhiteSpace(inputUser.Email))
            {
                //Atualizando informações do usuário
                user.Nome = inputUser.Nome;
                user.Email = inputUser.Email;
                user.Password = inputUser.Password;
                await db.SaveChangesAsync();
            } else
            {
                throw new ArgumentException("Nome, Senha e Email não podem ser vazios.");
            }
            
        }

        //Método para deletar usuário
        public async Task DeletarUsuario (Guid id, BaixumDB db)
        {
            //Buscando usuário
            Usuario user = await this.BuscarUsuarios(id, db);

            if (user != null)
            {
                //Deletando usuário
                db.Usuarios.Remove(user);
                await db.SaveChangesAsync();
            } else
            {
                //Lançando erro, caso usuário não exista
                throw new ArgumentException("Usuário não existe");
            }   
        }

        //Método para login
        public async Task<Usuario> Login(string password, string email, BaixumDB db)
        {
            //Buscando usuário
            Usuario user = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                //Verificando se senha é compatível
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }
                else
                {
                    //Lançando erro, caso senha esteja incorreta
                    throw new ArgumentException("Senha incorreta");
                }
            }
            else
            {
                //Lançando erro caso usuário não exista
                throw new ArgumentException("Usuário não existe");
            }
        }

        //Método para recuperar senha
        public async Task<Usuario> RecSenha(string email, BaixumDB db)
        {
            //Buscando usuário
            Usuario user = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                //Retornando usuário
                return user;
            }
            else
            {
                //Lançando erro, caso usuário não exista
                throw new ArgumentException("Usuário não existe");
            }

        }

    }
}
