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
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
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

        public AmazonS3Service(IOptions<AmazonS3Options> amazonS3Options, IDateTimeService dateTimeService)
        {
            if (!amazonS3Options.Value.Url.Contains("amazonaws.com"))
                throw new StorageException("StorageUrl should contain amazonaws.com");
            _dateTimeService = dateTimeService;
            _url = amazonS3Options.Value.Url;
            _bucket = amazonS3Options.Value.Bucket;
            var region = RegionEndpoint.GetBySystemName(amazonS3Options.Value.Region);

            var s3Config = new AmazonS3Config
            {
                Timeout = ClientConfig.MaxTimeout,
                RegionEndpoint = region
            };

            var credentials = new BasicAWSCredentials(amazonS3Options.Value.PublicKey, amazonS3Options.Value.SecretKey);

            _chunkThreshold = amazonS3Options.Value.ChunkThreshold;
            _client = new AmazonS3Client(credentials, s3Config);
        }

        public async Task RemoveAsync(string container, string blobName, CancellationToken cancellationToken = default)
        {
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

        public async Task SaveAsync(Stream stream, SaveBlobRequest request, CancellationToken cancellationToken = default)
        {
            long? length = stream.CanSeek 
                ? stream.Length 
                : null;

            var threshold = Math.Min(_chunkThreshold, 5000000000);
            var key = GetKeyName(request.Container, request.BlobName);

            if (length >= threshold)
            {
                var transferUtility = new TransferUtility(_client);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    InputStream = stream,
                    PartSize = 6291456,
                    Key = key,
                    ContentType = key.GetContentType(),
                    AutoCloseStream = true,
                    Headers = {ContentLength = length.Value},
                    CannedACL = S3CannedACL.PublicRead
                };
                
                if (request.Metadata is not null && request.Metadata.Count > 0)
                {
                    foreach (var (metaDataKey, metaDataValue) in request.Metadata)
                    {
                        fileTransferUtilityRequest.Metadata.Add(metaDataKey, metaDataValue);
                    }
                }

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
                    Key = key,
                    InputStream = stream,
                    ContentType = key.GetContentType(),
                    CannedACL = S3CannedACL.PublicRead,
                    AutoCloseStream = true
                };
                
                if (request.Metadata is not null && request.Metadata.Count > 0)
                {
                    foreach (var (metaDataKey, metaDataValue) in request.Metadata)
                    {
                        putRequest.Metadata.Add(metaDataKey, metaDataValue);
                    }
                }

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

        public string GetPresignedUrl(SaveBlobRequest request)
        {
            var expiration = _dateTimeService.Now.AddMinutes(5);
            var key = GetKeyName(request.Container, request.BlobName);
            var contentType = key.GetContentType();
            var presignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = expiration,
                ContentType = contentType,
                Verb = HttpVerb.PUT,
                ResponseHeaderOverrides =
                {
                    ContentDisposition = $"inline; filename={request.OriginalFileName}"
                },
            };

            if (request.Metadata is not null && request.Metadata.Count > 0)
            {
                foreach (var (metaDataKey, metaDataValue) in request.Metadata)
                {
                    presignedUrlRequest.Metadata.Add(metaDataKey, metaDataValue);
                }
            }
            
            var presignedUrl = _client.GetPreSignedURL(presignedUrlRequest);
            return presignedUrl;
        }

        private static string GetKeyName(string container, string blobName)
        {
            var path = Path.Combine(container, blobName);
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}