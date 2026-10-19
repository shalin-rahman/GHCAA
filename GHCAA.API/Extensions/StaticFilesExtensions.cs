using System.Text.RegularExpressions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace GHCAA.API.Extensions
{
    public static class StaticFilesExtensions
    {
        public const string NoStoreCacheControl = "no-cache, no-store, must-revalidate";
        public const string ImmutableCacheControl = "public, max-age=31536000, immutable";
        // Upload names carry a GUID (LocalFileStorageService), so a replaced file gets a new URL.
        // The day-long limit is there so a deleted upload does not linger in browsers for long.
        public const string UploadCacheControl = "public, max-age=86400";
        public const string RevalidateCacheControl = "no-cache";

        // /api/uploads/members/... serves the stored path uploads/members/...
        public const string UploadsRequestPath = "/api/uploads";
        public const string UploadsFolder = "uploads";
        private static readonly FileExtensionContentTypeProvider ContentTypes = new();

        // Angular's build names chunks like main-ABCD1234.js. The hash changes with the content,
        // so these files can be cached for a year (98.6).
        private static readonly Regex HashedBuildFile =
            new(@"-[A-Z0-9]{8}\.(js|css)$", RegexOptions.Compiled);

        public static bool IsHashedBuildFile(string fileName) => HashedBuildFile.IsMatch(fileName);

        public static WebApplication UseAppStaticFiles(
            this WebApplication app,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // 1. Serve wwwroot at root /. index.html must never be browser-cached.
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
                    {
                        SetNoStore(ctx.Context.Response);
                    }
                    else if (IsHashedBuildFile(ctx.File.Name))
                    {
                        ctx.Context.Response.Headers.CacheControl = ImmutableCacheControl;
                    }
                }
            });

            // 2. Map /api/uploads to the upload store: the disk, or the bucket when FileStorage:Provider is S3.
            var storageOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();
            if (storageOptions.UsesS3)
            {
                app.Use(ServeUploadFromStore);
            }
            else
            {
                var uploadsBasePath = configuration["FileStorage:BasePhysicalPath"]
                    ?? Path.Combine(environment.ContentRootPath, "wwwroot");
                var uploadsPhysicalPath = Path.Combine(uploadsBasePath, UploadsFolder);
                Directory.CreateDirectory(uploadsPhysicalPath);

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uploadsPhysicalPath),
                    RequestPath = UploadsRequestPath,
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = UploadCacheControl
                });
            }

            // 3. Fallback for missing uploads / images: return placeholder rather than 404
            var uploadImagePlaceholderPath = Path.Combine(
                environment.WebRootPath ?? "wwwroot", "assets", "placeholders", "image-placeholder.svg");
            var uploadImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

            app.Use(async (ctx, next) =>
            {
                var path = ctx.Request.Path;
                var lastSegment = path.Value?.Split('/').LastOrDefault() ?? "";
                var isFileLike = lastSegment.Contains('.');
                var isMissingUpload = path.StartsWithSegments(UploadsRequestPath) && isFileLike;
                var isMissingNonApiAsset = !path.StartsWithSegments("/api") && isFileLike;

                if (isMissingUpload || isMissingNonApiAsset)
                {
                    var isUploadPath = path.Value?.Contains("/uploads/", StringComparison.OrdinalIgnoreCase) == true;
                    var isImage = uploadImageExtensions.Any(ext => lastSegment.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
                    if (isUploadPath && isImage && File.Exists(uploadImagePlaceholderPath))
                    {
                        // The real file may come back (a restored disk), so the browser must ask again.
                        ctx.Response.Headers.CacheControl = RevalidateCacheControl;
                        ctx.Response.ContentType = "image/svg+xml";
                        await ctx.Response.SendFileAsync(uploadImagePlaceholderPath);
                        return;
                    }

                    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                await next(ctx);
            });

            return app;
        }

        // Bucket stand-in for the static-files route above. Only the public uploads tree is served;
        // secure_uploads goes through SecureFilesController. A missing file falls through to the
        // placeholder step.
        internal static async Task ServeUploadFromStore(HttpContext ctx, RequestDelegate next)
        {
            var request = ctx.Request;
            if (!(HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
                || !request.Path.StartsWithSegments(UploadsRequestPath, out var rest)
                || !rest.HasValue)
            {
                await next(ctx);
                return;
            }

            var storage = ctx.RequestServices.GetRequiredService<IFileStorageService>();
            var key = UploadsFolder + rest.Value;
            var file = await storage.OpenReadAsync(key, ctx.RequestAborted);

            // The gallery asks for thumbs/ copies without checking the bucket first, and photos
            // uploaded before thumbnails existed have none. The full photo stands in.
            var thumbSegment = "/" + Constants.Defaults.ThumbnailFolder + "/";
            if (file == null && key.Contains(thumbSegment, StringComparison.OrdinalIgnoreCase))
            {
                var index = key.LastIndexOf(thumbSegment, StringComparison.OrdinalIgnoreCase);
                file = await storage.OpenReadAsync(key[..index] + "/" + key[(index + thumbSegment.Length)..], ctx.RequestAborted);
            }

            if (file == null)
            {
                await next(ctx);
                return;
            }

            await using (file.Content)
            {
                var contentType = ContentTypes.TryGetContentType(key, out var known) ? known : "application/octet-stream";
                ctx.Response.Headers.CacheControl = UploadCacheControl;
                ctx.Response.Headers.XContentTypeOptions = "nosniff";

                // The file result answers If-None-Match and If-Modified-Since with a 304, sends no
                // body for HEAD, and serves Range requests, the same as the static-files route.
                var result = Results.Stream(file.Content, contentType,
                    lastModified: file.LastModified,
                    entityTag: EntityTagHeaderValue.TryParse(file.ETag, out var etag) ? etag : null,
                    enableRangeProcessing: true);
                await result.ExecuteAsync(ctx);
            }
        }

        public static void MapSpaFallback(this WebApplication app, IWebHostEnvironment environment)
        {
            var spaIndexPath = Path.Combine(environment.WebRootPath ?? "wwwroot", "index.html");
            if (File.Exists(spaIndexPath))
            {
                app.MapFallback("/{**path:nonfile}", async ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    SetNoStore(ctx.Response);
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.SendFileAsync(spaIndexPath);
                }).AllowAnonymous();
            }
        }

        private static void SetNoStore(HttpResponse response)
        {
            response.Headers.CacheControl = NoStoreCacheControl;
            response.Headers.Pragma = "no-cache";
            response.Headers.Expires = "0";
        }
    }
}
