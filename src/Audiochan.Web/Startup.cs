using Audiochan.Core;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure;
using Audiochan.Infrastructure.Storage.Options;
using Audiochan.Web.Configurations;
using Audiochan.Web.Middlewares;
using Audiochan.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Audiochan.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AmazonS3Options>(Configuration.GetSection(nameof(AmazonS3Options)));
            services.Configure<AudiochanOptions>(Configuration.GetSection(nameof(AudiochanOptions)));
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
            services.Configure<IdentityOptions>(Configuration.GetSection(nameof(IdentityOptions)));

            services
                .AddCoreServices()
                .AddInfraServices(Configuration, Environment.IsDevelopment())
                .ConfigureIdentity(Configuration)
                .ConfigureAuthentication(Configuration)
                .ConfigureAuthorization()
                .AddHttpContextAccessor()
                .AddScoped<ICurrentUserService, CurrentUserService>()
                .ConfigureControllers()
                .ConfigureRouting()
                .ConfigureCors()
                .ConfigureSwagger(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            if (env.IsDevelopment())
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    OnPrepareResponse = context =>
                    {
                        context.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    }
                });
            }
            
            app.UseRouting();
            app.UseCorsConfig();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwaggerConfig();
        }
    }
}
