using Microsoft.AspNetCore.ResponseCompression;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Extensions
{
    public static class CachingAndCompressionExtensions
    {
        // The query names the PublicContent actions read. Any other name is left out of the
        // cache key, so made-up query strings cannot add entries.
        private static readonly string[] PublicContentQueryKeys =
            { "page", "pageSize", "jobCategory", "query", "articleCategory", "postType", "group" };

        public static IServiceCollection AddAppCachingAndCompression(this IServiceCollection services)
        {
            // Response Compression (Brotli/Gzip)
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "image/svg+xml" });
            });

            // Output Caching (.NET 8+)
            // Base policy is NoCache. Actions opt in via [OutputCache(PolicyName = ...)]
            // when [AllowAnonymous] and returning the same body to all callers.
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(build => build.NoCache());

                // Reference data that changes rarely (lookups, governance/EC/constitution): 2 min
                // These actions read no query values, so any query string shares one entry.
                options.AddPolicy(OutputCachePolicies.PublicReference, build =>
                    build.Expire(TimeSpan.FromMinutes(2)).SetVaryByQuery(Array.Empty<string>()));

                // Content that admins edit more often (news, events, gallery, jobs, site content): 30s
                options.AddPolicy(OutputCachePolicies.PublicContent, build =>
                    build.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery(PublicContentQueryKeys));
            });

            return services;
        }
    }
}
