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

export const EC_ROLES_OPTIONS = EC_ROLES.map((label, index) => ({ value: index, label }));

export const ACADEMIC_CERTIFICATES = [
    'HSC',
    'Bachelor (Pass)',
    'Bachelor (Honours)',
    'Masters',
    'PGD',
    'PhD',
    'Medicine',
    'Engineering',
    'Law'
];

export const ACADEMIC_GROUPS = [
    'Science',
    'Arts & Humanities',
    'Business Studies'
];

export const ACADEMIC_SUBJECTS = [
    'None', 'Bengali', 'English', 'History', 'Islamic History & Culture',
    'Philosophy', 'Islamic Studies', 'Library Science', 'Economics',
    'Political Science', 'Sociology', 'Social Work', 'Anthropology',
    'Public Administration', 'Physics', 'Chemistry', 'Mathematics',
    'Statistics', 'Botany', 'Zoology', 'Geography & Environment',
    'Psychology', 'Soil Science', 'Accounting', 'Management',
    'Marketing', 'Finance & Banking', 'Fine Arts', 'Physical Education',
    'Business Administration', 'Computer', 'Civil', 'Mechanical',
    'Electrical', 'Medical', 'Dentestry', 'Engineering', 'Law',
    'Pharma', 'Agriculture', 'Textile', 'Lather', 'Education'
];

export const PROFESSIONAL_SECTORS = [
    'Ready-made Garments (RMG)',
    'Textiles & Spinning',
    'Pharmaceuticals',
    'Banking & Financial Services',
    'Information Technology (IT) & Software',
    'Telecommunications',
    'Agriculture & Crop Production',
    'Fisheries & Aquaculture',
    'Livestock & Poultry',
    'Agro-processing & Food Production',
    'Leather & Footwear',
    'Jute & Jute Goods',
    'Light Engineering',
    'Electronics & Electrical Appliances',
    'Real Estate & Housing',
    'Construction & Infrastructure',
    'Healthcare & Medical Services',
    'Education & Research',
    'Tourism & Hospitality',
    'Power, Energy & Mineral Resources',
    'Steel & Re-rolling',
    'Cement',
    'Ceramics',
    'Chemicals & Fertilizers',
    'Shipbuilding',
    'Transportation & Logistics',
    'Fast-Moving Consumer Goods (FMCG)',
    'Paper & Printing',
    'Plastic & Rubber Products',
    'Insurance',
    'Advertising & Media',
    'Legal & Consultancy Services',
    'Public Administration & Defense'
];

export const getAcademicYears = (): number[] => {
    const currentYear = new Date().getFullYear();
    const startYear = 1950;
    return Array.from({ length: currentYear - startYear + 1 }, (_, i) => currentYear - i);
};

export const IS_HSC = (cert: string | undefined | null) => cert === 'HSC';

export const ACADEMIC_DATA = {
    certificates: ACADEMIC_CERTIFICATES,
    groups: ACADEMIC_GROUPS,
    subjects: ACADEMIC_SUBJECTS,
    sectors: PROFESSIONAL_SECTORS,
    getYears: getAcademicYears
};

export const ensureValidAcademicData = (member: any) => {
    if (IS_HSC(member.highestCertificate) || IS_HSC(member.HighestCertificate)) {
        if (member.highestCertificateSubject) member.highestCertificateSubject = 'None';
        if (member.HighestCertificateSubject) member.HighestCertificateSubject = 'None';
    }
    if (IS_HSC(member.ghcLastCertificate) || IS_HSC(member.GHCLastCertificate)) {
        if (member.ghcLastCertificateSubject) member.ghcLastCertificateSubject = 'None';
        if (member.GHCLastCertificateSubject) member.GHCLastCertificateSubject = 'None';
    }
};

export const API_ENDPOINTS = {
    ADMIN: {
        MEMBERS: '/api/admin/members',
        MEMBERS_IMPORT: '/api/admin/members/import',
        STATS: '/api/admin/stats',
        COMMUNICATION: '/api/admin/comm',
        GOVERNANCE: '/api/admin/governance'
    },
    AUTH: {
        LOGIN: '/api/auth/login',
        REGISTER: '/api/auth/register',
        VERIFY_EMAIL: '/api/auth/verify-email',
        STATUS: '/api/auth/status'
    },
    EVENTS: '/api/events',
    GALLERY: '/api/gallery',
    NEWS: '/api/news',
    JOBS: '/api/jobs',
    PROFILE: '/api/profile',
    FINANCIALS: '/api/financials',
    MESSAGING: {
        RECENT: '/api/messaging/recent',
        HISTORY: '/api/messaging/history'
    },
    HUBS: {
        CHAT: '/hubs/chat'
    },
    NOTIFICATIONS: {
        BASE: '/api/notifications',
        READ_ALL: '/api/notifications/read-all'
    },
    NETWORKING: {
        COMMITTEE: '/api/networking/committee',
        COMMITTEE_PERIODS: '/api/networking/periods',
        SEARCH: '/api/networking/search',
        UPDATES: '/api/networking/updates'
    },
    LOOKUPS: '/api/lookups',
    LEDGER: '/api/ledger',
    CONTACT: '/api/contact',
    ASSISTANT: '/api/assistant/ask',
    THEMES: '/api/theme'
} as const;
