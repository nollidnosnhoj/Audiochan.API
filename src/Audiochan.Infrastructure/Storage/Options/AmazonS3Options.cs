﻿namespace Audiochan.Infrastructure.Storage.Options
{
    public record AmazonS3Options
    {
        public string Url { get; init; }
        public string PublicKey { get; init; }
        public string SecretKey { get; init; }
        public string Bucket { get; init; }
        public string Region { get; init; }
        public long ChunkThreshold { get; init; }
    }
}