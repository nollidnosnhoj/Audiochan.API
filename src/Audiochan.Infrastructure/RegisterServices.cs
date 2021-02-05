using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure.Images;
using Audiochan.Infrastructure.Security;
using Audiochan.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Audiochan.Infrastructure
{
    public static class RegisterServices
    {
        public static IServiceCollection AddInfraServices(this IServiceCollection services)
        {
            services.AddTransient<IAudioMetadataService, AudioMetadataService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IImageUploadService, ImageUploadService>();
            return services;
        }
    }
}