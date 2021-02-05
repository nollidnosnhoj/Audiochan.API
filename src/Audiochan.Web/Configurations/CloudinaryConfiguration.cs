using Audiochan.Core.Common.Options;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audiochan.Web.Configurations
{
    public static class CloudinaryConfiguration
    {
        public static IServiceCollection ConfigureCloudinary(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CloudinarySetting>(configuration.GetSection("Cloudinary"));
            return services;
        }
    }
}