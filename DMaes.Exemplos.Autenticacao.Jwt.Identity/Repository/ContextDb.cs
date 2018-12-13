using DMaes.Exemplos.Autenticacao.Jwt.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DMaes.Exemplos.Autenticacao.Jwt.Identity.Repository
{
    public class ContextDb : IdentityDbContext<UsuarioAplicativo>
    {
        public ContextDb(DbContextOptions<ContextDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
