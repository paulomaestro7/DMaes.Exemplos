using DMaes.Exemplos.Autenticacao.Jwt.Identity.Configuration;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Model;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Repository;
using DMaes.Exemplos.Autenticacao.Jwt.Identity.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DMaes.Exemplos.Autenticacao.Jwt.Identity
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("Configuration/Json/database.json", optional: true, reloadOnChange: true)
                .AddJsonFile("Configuration/Json/general.json", optional: true, reloadOnChange: true);

            ApplicationMode.Mode = (Mode)Enum.Parse(typeof(Mode), env.EnvironmentName);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddDbContext<ContextDb>(options =>
            options.UseSqlServer(Configuration.GetSection("ConnectionDevelopment").Value));

            services.AddIdentity<UsuarioAplicativo, IdentityRole>()
              .AddEntityFrameworkStores<ContextDb>()
              .AddDefaultTokenProviders();

            var configuracoesAssinatura = new ConfiguracoesAssinatura();
            services.AddSingleton(configuracoesAssinatura);

            var tokenConfigurations = new ConfiguracaoToken();
            new ConfigureFromConfigurationOptions<ConfiguracaoToken>(
                Configuration.GetSection("ConfiguracaoToken"))
                    .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = configuracoesAssinatura.Key;
                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;

                paramsValidation.ValidateIssuerSigningKey = true;

                paramsValidation.ValidateLifetime = true;

                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "PalmSoft Swagger",
                    Version = "v1",
                });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Utilizar a autenticação para geração do token em Token/Get e adicionar a chave conforme exemplo Example: Bearer {chave gerada pelo endpoint token/get}",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {{
                    "Bearer", Enumerable.Empty<string>()
                 }});


                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlFile);
            });

            services.AddMvc();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ContextDb Context, UserManager<UsuarioAplicativo> UserManager, RoleManager<IdentityRole> RoleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            new InicializadorIdentidade(Context, UserManager, RoleManager)
                .Initialize();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PalmSoft Swagger");
                c.DocExpansion(DocExpansion.None);
            });
        }

    }
}
