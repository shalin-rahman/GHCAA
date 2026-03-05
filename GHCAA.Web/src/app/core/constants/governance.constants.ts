export const EC_ROLES = [
    'President',
    'Vice President',
    'General Secretary',
    'Joint Secretary',
    'Treasurer',
    'Organizing Secretary',
    'Office Secretary',
    'Information Secretary',
    'Executive Member',
    'None'
] as const;

export type ECPositionType = typeof EC_ROLES[number];

export function getECPositionName(pos: number | string): string {
    if (typeof pos === 'number') {
        return EC_ROLES[pos] || 'Member';
    }
    return pos || 'None';
}
