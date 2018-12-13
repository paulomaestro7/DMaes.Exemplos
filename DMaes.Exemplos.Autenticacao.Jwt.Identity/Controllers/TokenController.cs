using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Configuration;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Model;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DMaes.Exemplos.Autenticacao.Jwt.Identity.Controllers
{
    [Route("Token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        public object Post([FromBody]Usuario usuario, [FromServices]UserManager<UsuarioAplicativo> userManager,
                           [FromServices]SignInManager<UsuarioAplicativo> signInManager,
                           [FromServices]ConfiguracoesAssinatura signingConfigurations,
                           [FromServices]ConfiguracaoToken tokenConfigurations)
        {
            bool credenciaisValidas = false;
            if (usuario != null && !String.IsNullOrWhiteSpace(usuario.Email))
            {
                var userIdentity = userManager.FindByNameAsync(usuario.Email).Result;

                if (userIdentity != null)
                {
                    var resultadoLogin = signInManager.CheckPasswordSignInAsync(userIdentity, usuario.Senha, false).Result;

                    if (resultadoLogin.Succeeded)
                    {
                        credenciaisValidas = userManager.IsInRoleAsync(userIdentity, Funcoes.Role_Fotos).Result;
                    }
                }
            }

            if (credenciaisValidas)
            {
                ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(usuario.Email, "Login"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Email)
                    }
                );

                DateTime dataCriacao = DateTime.Now;
                DateTime dataExpiracao = dataCriacao +
                    TimeSpan.FromSeconds(tokenConfigurations.Seconds);

                var handler = new JwtSecurityTokenHandler();
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Issuer = tokenConfigurations.Issuer,
                    Audience = tokenConfigurations.Audience,
                    SigningCredentials = signingConfigurations.SigningCredentials,
                    Subject = identity,
                    NotBefore = dataCriacao,
                    Expires = dataExpiracao
                });
                var token = handler.WriteToken(securityToken);

                return new
                {
                    authenticated = true,
                    created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                    expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                    accessToken = token,
                    message = "OK"
                };
            }
            else
            {
                return new
                {
                    authenticated = false,
                    message = "Falha ao autenticar"
                };
            }
        }
    }
}
