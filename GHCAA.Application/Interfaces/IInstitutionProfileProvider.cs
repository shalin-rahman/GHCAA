using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    // Reads the institution profile pack selected by the ORG_PROFILE environment variable
    // (profiles/<name>/*.json, falling back file-by-file to profiles/default/) so a second
    // institution can be stood up with a config folder instead of a source change. See
    // docs/WHITE_LABEL_PLAN.md and docs/TODO.md Work Package 62. Nothing consumes this yet —
    // OrgConfigService still builds its defaults in code (62.6, a separate phase); this only
    // loads and validates the pack so that phase has something real to read from.
    public interface IInstitutionProfileProvider
    {
        // The profile actually resolved (ORG_PROFILE's value, or "default" if unset).
        string ProfileName { get; }

        // The org-config.json shape for the resolved profile, loaded and cached once at
        // startup. Throws at construction if neither the named profile nor the default one
        // has a readable, valid org-config.json.
        OrgConfigDto OrgConfigDefaults { get; }
    }
}
