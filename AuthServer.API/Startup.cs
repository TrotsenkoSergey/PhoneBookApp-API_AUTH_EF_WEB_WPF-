using AuthServer.API.Models;
using AuthServer.API.Services.Authenticators;
using AuthServer.API.Services.PasswordHashes;
using AuthServer.API.Services.RefreshTokenRepositories;
using AuthServer.API.Services.TokenGenerators;
using AuthServer.API.Services.TokenValidators;
using AuthServer.API.Services.UserRepositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

            string connectionString = _configuration.GetConnectionString("sqlite");
            services.AddDbContext<AuthenticationDbContext>(o => o.UseSqlite(connectionString));

            services.AddSingleton<TokenGenerator>();
            services.AddSingleton<AccessTokenGenerator>();
            services.AddSingleton<RefreshTokenGenerator>();
            services.AddSingleton<RefreshTokenValidator>();
            services.AddScoped<Authenticator>();
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IUserRepository, DataBaseUserRepository>();
            services.AddScoped<IRefreshTokenRepository, DataBaseRefreshTokenRepository>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters()
                   {
                       IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                authenticationConfiguration.AccessTokenSecret
                                )),
                       ValidIssuer = authenticationConfiguration.Issuer,
                       ValidAudience = authenticationConfiguration.Audience,
                       ValidateIssuerSigningKey = true,
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ClockSkew = TimeSpan.Zero
                   };
               });
        }

       
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
