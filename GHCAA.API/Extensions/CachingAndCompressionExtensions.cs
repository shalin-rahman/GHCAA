using GHCAA.Domain;
using static GHCAA.Domain.Constants;
using Microsoft.AspNetCore.ResponseCompression;

namespace GHCAA.API.Extensions
{
    public static class CachingAndCompressionExtensions
    {
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
                options.AddPolicy(OutputCachePolicies.PublicReference, build =>
                    build.Expire(TimeSpan.FromMinutes(2)).SetVaryByQuery("*"));

                // Content that admins edit more often (news, events, gallery, jobs, site content): 30s
                options.AddPolicy(OutputCachePolicies.PublicContent, build =>
                    build.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
            });

            return services;
        }
    }
}
