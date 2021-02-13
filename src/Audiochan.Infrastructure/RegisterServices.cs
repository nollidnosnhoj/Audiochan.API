using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure.Image;
using Audiochan.Infrastructure.Security;
using Audiochan.Infrastructure.Shared;
using Audiochan.Infrastructure.Upload;
using Microsoft.Extensions.DependencyInjection;

namespace Audiochan.Infrastructure
{
    public static class RegisterServices
    {
        public static IServiceCollection AddInfraServices(this IServiceCollection services)
        {
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IUploadService, UploadService>();
            return services;
        }
    }
}