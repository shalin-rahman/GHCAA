using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GHCAA.Infrastructure.Services
{
    // Keeps uploads in an S3-compatible bucket. The object key is the stored path itself
    // (uploads/members/12/photo/...), so rows saved under local storage keep working after the
    // files are copied into the bucket with the same layout.
    public class S3FileStorageService : FileStorageServiceBase
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        public S3FileStorageService(IAmazonS3 s3, IOptions<FileStorageOptions> options, ILogger<S3FileStorageService> logger)
            : base(options.Value, logger)
        {
            _s3 = s3;
            _bucket = _options.S3.BucketName;
            if (string.IsNullOrWhiteSpace(_bucket))
                throw new InvalidOperationException("FileStorage:S3:BucketName must be set when FileStorage:Provider is S3.");
        }

        protected override async Task WriteAsync(string relativePath, Stream content, bool isSecure, CancellationToken cancellationToken)
        {
            // The SDK needs the length up front. A form upload stream cannot always seek.
            Stream body = content;
            MemoryStream? buffer = null;
            if (!content.CanSeek)
            {
                buffer = new MemoryStream();
                await content.CopyToAsync(buffer, cancellationToken);
                buffer.Position = 0;
                body = buffer;
            }

            try
            {
                await _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = Normalize(relativePath).TrimStart('/'),
                    InputStream = body,
                    AutoCloseStream = false,
                    ContentType = ContentTypeFor(relativePath),
                    // Some S3-compatible services reject the chunked signed upload the SDK uses by default.
                    UseChunkEncoding = false,
                }, cancellationToken);
            }
            finally
            {
                buffer?.Dispose();
            }
        }

        // A HEAD per gallery item would slow the gallery list down, so the path is always offered.
        // The /api/uploads proxy serves the full photo when the thumbnail key is missing.
        protected override bool ThumbnailExists(string thumbnailRelativePath) => true;

        public override async Task<StoredFile?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var key = KeyFor(relativePath);
            if (key == null) return null;

            try
            {
                var response = await _s3.GetObjectAsync(_bucket, key, cancellationToken);
                // The SDK may hand back an unmarked time. S3 always sends UTC.
                DateTimeOffset? lastModified = response.LastModified is not { } modified ? null
                    : modified.Kind == DateTimeKind.Unspecified ? new DateTimeOffset(modified, TimeSpan.Zero)
                    : new DateTimeOffset(modified);
                return new StoredFile(new ObjectStream(_s3, _bucket, key, response), lastModified, response.ETag);
            }
            // A wrong bucket name is also a 404. It must fail loudly, not show placeholders for every photo.
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound && ex.ErrorCode != NoSuchBucketCode)
            {
                return null;
            }
        }

        private const string NoSuchBucketCode = "NoSuchBucket";

        public override async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            using var probe = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            probe.CancelAfter(TimeSpan.FromSeconds(Constants.Deploy.StorageProbeTimeoutSeconds));
            try
            {
                await _s3.ListObjectsV2Async(new ListObjectsV2Request { BucketName = _bucket, MaxKeys = 1 }, probe.Token);
                return true;
            }
            // Bad keys, a wrong endpoint, a network fault and a timeout all throw different types.
            // Each one means the same thing to a health check. Only the caller giving up rethrows.
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Storage bucket {Bucket} could not be reached", _bucket);
                return false;
            }
        }

        public override async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var key = KeyFor(relativePath);
            if (key == null) return;

            await _s3.DeleteObjectAsync(_bucket, key, cancellationToken);

            // A gallery photo's thumbnail sits beside it under thumbs/ with the same name. The photo
            // is already gone, so a thumbnail that will not delete is only logged.
            if (!key.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase) || IsThumbnailPath(key)) return;

            var thumbKey = ThumbnailPathFor(key);
            try
            {
                await _s3.DeleteObjectAsync(_bucket, thumbKey, cancellationToken);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete thumbnail {Key}", thumbKey);
            }
        }

        // Only keys under the two upload trees. Stored paths also include seeded assets such as
        // assets/images/..., which live in the web root and never in the bucket.
        private string? KeyFor(string relativePath)
        {
            var cleaned = CleanRelativePath(relativePath);
            if (cleaned == null) return null;
            return cleaned.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase)
                || cleaned.StartsWith(SecurePrefix, StringComparison.OrdinalIgnoreCase)
                ? cleaned
                : null;
        }

        private static string ContentTypeFor(string path) =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };

        // Reads the object from the open GetObject response. A seek away from the read position
        // closes that response and asks the bucket for the rest from the new position, which is
        // how a range request is served without buffering the file. The ETag must still match, so
        // a file replaced between the two requests fails instead of mixing two versions.
        // Disposing the stream closes the response and its HTTP connection.
        private sealed class ObjectStream : Stream
        {
            private readonly IAmazonS3 _s3;
            private readonly string _bucket;
            private readonly string _key;
            private readonly string? _etag;
            private GetObjectResponse? _response;
            private long _responsePosition;
            private long _position;

            public ObjectStream(IAmazonS3 s3, string bucket, string key, GetObjectResponse response)
            {
                _s3 = s3;
                _bucket = bucket;
                _key = key;
                _etag = response.ETag;
                _response = response;
                Length = response.ContentLength;
            }

            public override long Length { get; }
            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;

            public override long Position
            {
                get => _position;
                set => Seek(value, SeekOrigin.Begin);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                var target = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => _position + offset,
                    _ => Length + offset,
                };
                if (target < 0) throw new IOException("Cannot seek before the start of the object.");
                _position = target;
                return _position;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                if (buffer.Length == 0 || _position >= Length) return 0;

                if (_response == null || _responsePosition != _position)
                {
                    _response?.Dispose();
                    _response = null;
                    var request = new GetObjectRequest
                    {
                        BucketName = _bucket,
                        Key = _key,
                        ByteRange = new ByteRange(_position, Length - 1),
                        EtagToMatch = _etag,
                    };
                    _response = await _s3.GetObjectAsync(request, cancellationToken);
                    _responsePosition = _position;
                }

                var read = await _response.ResponseStream.ReadAsync(buffer, cancellationToken);
                _position += read;
                _responsePosition += read;
                return read;
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

            // Callers here read asynchronously. A synchronous read still works, it just blocks.
            public override int Read(byte[] buffer, int offset, int count) =>
                ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

            public override void Flush() { }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _response?.Dispose();
                    _response = null;
                }
                base.Dispose(disposing);
            }
        }
    }
}
