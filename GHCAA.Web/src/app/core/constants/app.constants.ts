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

export const MEMBERSHIP_STATUS_MAP: Record<string | number, { label: string, class: string }> = {
    'Applied': { label: 'Pending', class: 'pending' },
    0: { label: 'Pending', class: 'pending' },
    'Active': { label: 'Active', class: 'active' },
    1: { label: 'Active', class: 'active' },
    'InactivePayment': { label: 'Inactive', class: 'inactive' },
    2: { label: 'Inactive', class: 'inactive' },
    'InactiveResigned': { label: 'Resigned', class: 'resigned' },
    3: { label: 'Resigned', class: 'resigned' },
    'Terminated': { label: 'Terminated', class: 'terminated' },
    4: { label: 'Terminated', class: 'terminated' }
};

export const MEMBERSHIP_TYPES = [
    'Founding',
    'Executive',
    'General',
    'Associate',
    'Honorary',
    'Advisory'
];

// Membership options moved to grouped section below


export const MEMBER_CATEGORIES = [
    'None',
    'Lifelong',
    'Donor',
    'Patron'
];

export const BLOOD_GROUPS = [
    'A+', 'A-', 'B+', 'B-', 'O+', 'O-', 'AB+', 'AB-'
];

export const BLOOD_GROUP_OPTIONS = [
    { value: 'APositive', label: 'A+' },
    { value: 'ANegative', label: 'A-' },
    { value: 'BPositive', label: 'B+' },
    { value: 'BNegative', label: 'B-' },
    { value: 'OPositive', label: 'O+' },
    { value: 'ONegative', label: 'O-' },
    { value: 'ABPositive', label: 'AB+' },
    { value: 'ABNegative', label: 'AB-' }
];

export const GENDERS = [
    'Male', 'Female', 'Other'
];

export const GENDER_OPTIONS = [
    { value: 'Male', label: 'Male' },
    { value: 'Female', label: 'Female' },
    { value: 'Other', label: 'Other' }
];

export const JOB_CATEGORIES = [
    { id: 0, name: 'IT & Software Development' },
    { id: 1, name: 'Finance & Banking' },
    { id: 2, name: 'Engineering & Construction' },
    { id: 3, name: 'Marketing & Sales' },
    { id: 4, name: 'Education & Research' },
    { id: 5, name: 'Healthcare & Pharma' },
    { id: 6, name: 'Govt. & Public Sector' },
    { id: 7, name: 'Mentorship & Career Guidance' },
    { id: 8, name: 'Other Opportunities' }
];

export type ECPositionType = typeof EC_ROLES[number];

export function getECPositionName(pos: number | string): string {
    if (pos === null || pos === undefined || pos === 'None' || pos === '0' || pos === 0) return 'None';

    if (typeof pos === 'number') {
        return EC_ROLES[pos] || 'Member';
    }

    // Handle numeric string "1", "2" etc
    if (/^\d+$/.test(pos)) {
        return EC_ROLES[parseInt(pos)] || 'Member';
    }

    // If it's a string from the enum name, try to format it with spaces
    // e.g. MediaCulturalAndSportsSecretary -> Media Cultural And Sports Secretary
    return pos.replace(/([A-Z])/g, ' $1').trim();
}

export function getStatusLabel(status: string | number): string {
    // Try original, then try parsing as number if it's a string digit
    const res = MEMBERSHIP_STATUS_MAP[status];
    if (res) return res.label;

    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return MEMBERSHIP_STATUS_MAP[parseInt(status)]?.label || 'Unknown';
    }
    return 'Unknown';
}

export function getStatusClass(status: string | number): string {
    const res = MEMBERSHIP_STATUS_MAP[status];
    if (res) return res.class;

    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return MEMBERSHIP_STATUS_MAP[parseInt(status)]?.class || '';
    }
    return '';
}

export function getCategoryLabel(cat: string | number): string {
    if (typeof cat === 'number') return MEMBER_CATEGORIES[cat] || 'None';
    return cat || 'None';
}

export function getMembershipTypeLabel(type: string | number): string {
    if (typeof type === 'number') return MEMBERSHIP_TYPES[type] || 'General';
    return type || 'General';
}

export const EC_ROLES_OPTIONS = EC_ROLES.map((label, index) => ({ value: index, label }));

export const MEMBERSHIP_STATUS_OPTIONS = [
    { value: 'all', label: 'All Statuses' },
    { value: 'Applied', label: 'Pending Audit' },
    { value: 'Active', label: 'Active Member' },
    { value: 'InactivePayment', label: 'Inactive (Payment)' },
    { value: 'InactiveResigned', label: 'Inactive (Resigned)' },
    { value: 'Terminated', label: 'Terminated' }
];

export const MEMBERSHIP_TYPE_OPTIONS = [
    { value: 'Founding', label: 'Founding Member' },
    { value: 'Executive', label: 'Executive Committee' },
    { value: 'General', label: 'General Member' },
    { value: 'Associate', label: 'Associate Member' },
    { value: 'Honorary', label: 'Honorary Member' },
    { value: 'Advisory', label: 'Advisory Member' }
];

export const MEMBER_CATEGORY_OPTIONS = [
    { value: 'None', label: 'No Special Status' },
    { value: 'Lifelong', label: 'Lifelong Member' },
    { value: 'Donor', label: 'Donor Member' },
    { value: 'Patron', label: 'Patron Member' }
];

export const FINANCIAL_CATEGORY_OPTIONS = [
    { value: 'MembershipFee', label: 'Yearly Membership Fee' },
    { value: 'RegistrationFee', label: 'Registration Fee' },
    { value: 'Donation', label: 'Donation' },
    { value: 'Event', label: 'Event Fee' },
    { value: 'Other', label: 'Other' }
];

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
    // Process flat structure (Legacy/Registration)
    if (IS_HSC(member.highestCertificate) || IS_HSC(member.HighestCertificate)) {
        if (member.highestCertificateSubject) member.highestCertificateSubject = 'None';
        if (member.HighestCertificateSubject) member.HighestCertificateSubject = 'None';
    }
    if (IS_HSC(member.ghcLastCertificate) || IS_HSC(member.GHCLastCertificate)) {
        if (member.ghcLastCertificateSubject) member.ghcLastCertificateSubject = 'None';
        if (member.GHCLastCertificateSubject) member.GHCLastCertificateSubject = 'None';
    }

    // Process AcademicHistory array (New LinkedIn style)
    if (member.academicHistory && Array.isArray(member.academicHistory)) {
        member.academicHistory.forEach((item: any) => {
            if (IS_HSC(item.degree)) {
                item.subject = 'None';
            }
        });
    }
    if (member.AcademicHistory && Array.isArray(member.AcademicHistory)) {
        member.AcademicHistory.forEach((item: any) => {
            if (IS_HSC(item.degree)) {
                item.subject = 'None';
            }
        });
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
    THEMES: '/api/theme',
    PAYMENT_CONFIG: {
        BASE: '/api/payment-config',
        PUBLIC: '/api/payment-config/active'
    }
} as const;
