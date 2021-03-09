using Audiochan.API.Middlewares;
using Audiochan.API.Services;
using Audiochan.Core;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure;
using Audiochan.Infrastructure.Storage.Options;
using Audiochan.API.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Audiochan.API
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
                .AddMemoryCache()
                .AddCoreServices()
                .AddInfraServices(Configuration, Environment.IsDevelopment())
                .ConfigureIdentity(Configuration)
                .ConfigureAuthentication(Configuration)
                .ConfigureAuthorization()
                .AddHttpContextAccessor()
                .AddScoped<ICurrentUserService, CurrentUserService>()
                .ConfigureControllers()
                .ConfigureRouting()
                .ConfigureRateLimiting(Configuration)
                .ConfigureCors()
                .ConfigureSwagger(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorsConfig();
            app.UseRateLimiting();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
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
