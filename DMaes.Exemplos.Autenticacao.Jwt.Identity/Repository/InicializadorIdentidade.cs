using DMaes.Exemplos.Autenticacao.Jwt.Identity.Configuration;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Model;
using Microsoft.AspNetCore.Identity;
using System;

namespace DMaes.Exemplos.Autenticacao.Jwt.Identity.Repository
{
    public class InicializadorIdentidade
    {
        private readonly ContextDb _context;
        private readonly UserManager<UsuarioAplicativo> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public InicializadorIdentidade(ContextDb Context, UserManager<UsuarioAplicativo> UserManager, RoleManager<IdentityRole> roleManager)
        {
            _context = Context;
            _userManager = UserManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            if (_context.Database.EnsureCreated())
            {
                if (!_roleManager.RoleExistsAsync(Funcoes.Role_Fotos).Result)
                {
                    var resultado = _roleManager.CreateAsync(new IdentityRole(Funcoes.Role_Fotos)).Result;
                    if (!resultado.Succeeded)
                    {
                        throw new Exception($"Erro durante a criação da role {Funcoes.Role_Fotos}.");
                    }
                }

                CreateUser(
                    new UsuarioAplicativo()
                    {
                        UserName = "maestro",
                        Email = "maestro@dmaes.com.br",
                        EmailConfirmed = true
                    }, "Abcd1234!", Funcoes.Role_Fotos);
                CreateUser(
                    new UsuarioAplicativo()
                    {
                        UserName = "paulo.maestro",
                        Email = "maestro@paulomaestro.com.br",
                        EmailConfirmed = true
                    }, "Abcd1234!");
            }
        }

        private void CreateUser(UsuarioAplicativo User, string Password, string InitialRole = null)
        {
            if (_userManager.FindByNameAsync(User.UserName).Result == null)
            {
                var resultado = _userManager.CreateAsync(User, Password).Result;

                if (resultado.Succeeded && !String.IsNullOrWhiteSpace(InitialRole))
                {
                    _userManager.AddToRoleAsync(User, InitialRole).Wait();
                }
            }
        }
    }
}
