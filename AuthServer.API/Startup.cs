using AuthServer.API.Models;
using AuthServer.API.Services.Authenticators;
using AuthServer.API.Services.PasswordHashes;
using AuthServer.API.Services.RefreshTokenRepositories;
using AuthServer.API.Services.TokenGenerators;
using AuthServer.API.Services.TokenValidators;
using AuthServer.API.Services.UserRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            AuthenticationConfiguration authenticationConfiguration = new AuthenticationConfiguration();
            _configuration.Bind("Authentication", authenticationConfiguration);
            services.AddSingleton(authenticationConfiguration);

            services.AddSingleton<TokenGenerator>();
            services.AddSingleton<AccessTokenGenerator>();
            services.AddSingleton<RefreshTokenGenerator>();
            services.AddSingleton<RefreshTokenValidator>();
            services.AddSingleton<Authenticator>();
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IRefreshTokenRepository, InMemoryRefreshTokenRepository>();
        }

       
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
