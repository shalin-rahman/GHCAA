using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

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

            // 2. Map /api/uploads to physical storage path
            var uploadsBasePath = configuration["FileStorage:BasePhysicalPath"]
                ?? Path.Combine(environment.ContentRootPath, "wwwroot");
            var uploadsPhysicalPath = Path.Combine(uploadsBasePath, "uploads");
            Directory.CreateDirectory(uploadsPhysicalPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPhysicalPath),
                RequestPath = "/api/uploads",
                OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = UploadCacheControl
            });

            // 3. Fallback for missing uploads / images: return placeholder rather than 404
            var uploadImagePlaceholderPath = Path.Combine(
                environment.WebRootPath ?? "wwwroot", "assets", "placeholders", "image-placeholder.svg");
            var uploadImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

            app.Use(async (ctx, next) =>
            {
                var path = ctx.Request.Path;
                var lastSegment = path.Value?.Split('/').LastOrDefault() ?? "";
                var isFileLike = lastSegment.Contains('.');
                var isMissingUpload = path.StartsWithSegments("/api/uploads") && isFileLike;
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
