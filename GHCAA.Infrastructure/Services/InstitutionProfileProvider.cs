using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace GHCAA.Infrastructure.Services
{
    public class InstitutionProfileProvider : IInstitutionProfileProvider
    {
        private const string DefaultProfileName = "default";
        private const string OrgConfigFileName = "org-config.json";

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public string ProfileName { get; }
        public OrgConfigDto OrgConfigDefaults { get; }

        public InstitutionProfileProvider(IConfiguration configuration, IHostEnvironment environment)
        {
            // An unset ORG_PROFILE used to fall back to the neutral sample pack. On a live
            // deployment that rebrands a real association, so it now stops the app at boot (62.6b).
            var configured = configuration["ORG_PROFILE"];
            if (string.IsNullOrWhiteSpace(configured))
            {
                throw new InvalidOperationException(
                    "ORG_PROFILE is not set. Set it to the institution's profile folder under profiles/ " +
                    "(for example ORG_PROFILE=ghc), or ORG_PROFILE=default for the neutral sample pack.");
            }
            ProfileName = configured;

            var profilesRoot = ResolveProfilesRoot(environment.ContentRootPath);
            var path = FindFile(profilesRoot, ProfileName, OrgConfigFileName)
                ?? FindFile(profilesRoot, DefaultProfileName, OrgConfigFileName);

            if (path == null)
            {
                throw new InvalidOperationException(
                    $"Institution profile '{ProfileName}' has no {OrgConfigFileName}, and neither does " +
                    $"the '{DefaultProfileName}' fallback. Looked under: {profilesRoot}. " +
                    "A profile pack needs at least profiles/<name>/org-config.json or profiles/default/org-config.json.");
            }

            OrgConfigDefaults = JsonSerializer.Deserialize<OrgConfigDto>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException($"{path} is present but did not deserialize to a valid org-config.json.");
        }

        // Docker's final image has "profiles/" copied directly under the content root (WORKDIR
        // /app). A local `dotnet run` from GHCAA.API/ has the content root one level below the
        // repo root, where "profiles/" actually lives — so the parent directory is checked too.
        private static string ResolveProfilesRoot(string contentRootPath)
        {
            var direct = Path.Combine(contentRootPath, "profiles");
            if (Directory.Exists(direct)) return direct;

            var parent = Directory.GetParent(contentRootPath);
            if (parent != null)
            {
                var viaParent = Path.Combine(parent.FullName, "profiles");
                if (Directory.Exists(viaParent)) return viaParent;
            }

            return direct; // doesn't exist; used only for the error message above
        }

        private static string? FindFile(string profilesRoot, string profileName, string fileName)
        {
            var path = Path.Combine(profilesRoot, profileName, fileName);
            return File.Exists(path) ? path : null;
        }
    }
}
