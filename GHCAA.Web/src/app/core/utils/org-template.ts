import { OrgConfig } from '../models/org-config.model';

/**
 * Replaces `{branding.fullName}`-style placeholders in a string with the matching
 * field off the live OrgConfig. Used for route titles/descriptions and the SEO
 * fallback text so those strings resolve per-institution instead of being
 * hardcoded per route. A placeholder with no matching field, or a null config,
 * is left in the output unchanged rather than throwing.
 */
export function interpolateOrgTemplate(template: string, cfg: OrgConfig | null): string {
  if (!cfg) return template;
  return template.replace(/\{([\w.]+)\}/g, (match, path: string) => {
    const value = path.split('.').reduce<unknown>((obj: any, key: string) => obj?.[key], cfg);
    return value === undefined || value === null ? match : String(value);
  });
}
