using Microsoft.Extensions.FileProviders;

namespace GHCAA.API.Extensions
{
    public static class StaticFilesExtensions
    {
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
                        ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                        ctx.Context.Response.Headers.Pragma = "no-cache";
                        ctx.Context.Response.Headers.Expires = "0";
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
                RequestPath = "/api/uploads"
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

                    ctx.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                    ctx.Response.Headers.Pragma = "no-cache";
                    ctx.Response.Headers.Expires = "0";
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.SendFileAsync(spaIndexPath);
                }).AllowAnonymous();
            }
        }
    }
}
