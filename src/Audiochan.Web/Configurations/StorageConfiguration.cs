using Amazon.S3;
using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure.Storage;
using Audiochan.Infrastructure.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audiochan.Web.Configurations
{
    public static class StorageConfiguration
    {
        public static IServiceCollection ConfigureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AmazonS3Options>(configuration.GetSection(nameof(AmazonS3Options)));
            services.AddAWSService<IAmazonS3>();
            services.AddTransient<IStorageService, AmazonS3Service>();
            return services;
        }
    }
}