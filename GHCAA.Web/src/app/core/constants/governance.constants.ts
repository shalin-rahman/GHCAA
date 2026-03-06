export const EC_ROLES = [
    'None',
    'President',
    'Vice President',
    'General Secretary',
    'Office Secretary',
    'Joint Secretary -1',
    'Joint Secretary -2',
    'Treasurer',
    'Media Cultural & Sports Secretary',
    'Organizational Secretary',
    'Information and Technology Secretary',
    'Member-1',
    'Member-2',
    'Law Secretary',
    'Immediate Past President',
    'Institutional Representative'
] as const;

export type ECPositionType = typeof EC_ROLES[number];

export function getECPositionName(pos: number | string): string {
    if (typeof pos === 'number') {
        return EC_ROLES[pos] || 'Member';
    }
    return pos || 'None';
}
