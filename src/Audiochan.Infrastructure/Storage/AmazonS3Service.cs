using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Audiochan.Core.Common.Exceptions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Interfaces;
using Audiochan.Infrastructure.Storage.Extensions;
using Audiochan.Infrastructure.Storage.Options;
using Microsoft.Extensions.Options;

namespace Audiochan.Infrastructure.Storage
{
    public class AmazonS3Service : IStorageService
    {
        private readonly IAmazonS3 _client;
        private readonly IDateTimeService _dateTimeService;
        private readonly string _bucket;
        private readonly long _chunkThreshold;
        private readonly string _url;

        public AmazonS3Service(
            IOptions<AmazonS3Options> amazonS3Options,
            IOptions<AudiochanOptions> audiochanOptions,
            IDateTimeService dateTimeService)
        {
            if (!audiochanOptions.Value.StorageUrl.Contains("amazonaws.com"))
                throw new StorageException("StorageUrl should contain amazonaws.com");
            _dateTimeService = dateTimeService;
            _url = audiochanOptions.Value.StorageUrl;
            _bucket = amazonS3Options.Value.Bucket;
            var region = RegionEndpoint.GetBySystemName(amazonS3Options.Value.Region);

            var s3Config = new AmazonS3Config
            {
                Timeout = ClientConfig.MaxTimeout,
                RegionEndpoint = region
            };

            var credentials = new BasicAWSCredentials(
                amazonS3Options.Value.PublicKey, 
                amazonS3Options.Value.SecretKey);

            _chunkThreshold = amazonS3Options.Value.ChunkThreshold;
            _client = new AmazonS3Client(credentials, s3Config);
        }

        public async Task<string> GetPresignedUrlAsync(string container, string blobName, 
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => GetPresignedUrl(container, blobName), cancellationToken);
        }

        public async Task RemoveAsync(string path, CancellationToken cancellationToken = default)
        {
            var (container, blobName) = path.GetBlobPath();
            var key = GetKeyName(container, blobName);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            try
            {
                await _client.DeleteObjectAsync(deleteRequest, cancellationToken);
            }
            catch (AmazonS3Exception ex)
            {
                throw new StorageException(ex.Message);
            }
        }

        public async Task SaveAsync(string container, string blobName, Stream stream, 
            bool overwrite = true,
            CancellationToken cancellationToken = default)
        {
            long? length = stream.CanSeek ? stream.Length : null;

            var threshold = Math.Min(_chunkThreshold, 5000000000);

            if (length >= threshold)
            {
                var transferUtility = new TransferUtility(_client);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    InputStream = stream,
                    PartSize = 6291456,
                    Key = GetKeyName(container, blobName),
                    ContentType = blobName.GetContentType(),
                    AutoCloseStream = true,
                    Headers = {ContentLength = length.Value},
                    CannedACL = S3CannedACL.PublicRead
                };

                try
                {
                    await transferUtility.UploadAsync(fileTransferUtilityRequest, cancellationToken);
                }
                catch (AmazonS3Exception ex)
                {
                    throw new StorageException(ex.Message);
                }
            }
            else
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = GetKeyName(container, blobName),
                    InputStream = stream,
                    ContentType = blobName.GetContentType(),
                    CannedACL = S3CannedACL.PublicRead,
                    AutoCloseStream = true
                };

                try
                {
                    await _client.PutObjectAsync(putRequest, cancellationToken);
                }
                catch (AmazonS3Exception ex)
                {
                    throw new StorageException(ex.Message);
                }
            }
        }

        public async Task<BlobDto> GetAsync(string container, string blobName,
            CancellationToken cancellationToken = default)
        {
            var key = GetKeyName(container, blobName);

            try
            {
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucket,
                    Key = key
                };

                var response = await _client.GetObjectMetadataAsync(getRequest, cancellationToken);

                return new BlobDto(true, container, blobName, $"{_url}/{key}", response.ContentLength);
            }
            catch (AmazonS3Exception ex)
            {
                throw new StorageException(ex.Message);
            }
        }

        private string GetPresignedUrl(string container, string blobName)
        {
            var expiration = _dateTimeService.Now.AddMinutes(5);
            var key = GetKeyName(container, blobName);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = expiration
            };
            var presignedUrl = _client.GetPreSignedURL(request);
            return presignedUrl;
        }

        private static string GetKeyName(string container, string blobName)
        {
            container = container.Replace(Path.DirectorySeparatorChar, '/');
            return string.IsNullOrWhiteSpace(container)
                ? blobName
                : $"{container}/{blobName}";
        }
    }
}