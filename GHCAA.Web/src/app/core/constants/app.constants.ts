// 82.44: the shared debounce delay for live search inputs (governance member search, directory,
// jobs, messages member picker, requests). Keep one value so every search field waits the same
// amount of time before firing.
export const SEARCH_DEBOUNCE_MS = 300;

export const EC_ROLES = [
    'None',
    'President',
    'Vice President',
    'General Secretary',
    'Office Secretary',
    'Organizational Secretary',
    'Information and Technology Secretary',
    'Law Secretary',
    'Media Cultural & Sports Secretary',    
    'Member-1',
    'Member-2',
    'Immediate Past President',
    'Institutional Representative'
] as const;

// 82.42: the /lookups/{group} route takes any string and matches it against the Lookups
// table's LookupGroup column — these are the exact group names GHCAA.Mobile's DropdownService
// already calls with, so both clients ask the API for the same rows.
export const LOOKUP_GROUPS = {
    MembershipStatus: 'MembershipStatus',
    MemberCategory: 'MemberCategory',
    MembershipType: 'MembershipType',
    Gender: 'Gender',
    BloodGroup: 'BloodGroup',
    JobCategory: 'JobCategory',
    PassingYear: 'PassingYear'
} as const;

export const DATE_FORMAT = 'dd-MM-yyyy';
export const DATE_REGEX = /^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-\d{4}$/;

export const DEVELOPER_INFO = {
    name: 'md habibur rahman shalin',
    email: 'shalin.rahman@gmail.com',
    link: 'mailto:shalin.rahman@gmail.com'
};

// 82.42: Applied/InactivePayment used to read "Pending"/"Inactive" here but "Pending Approval"/
// "Inactive (Unpaid)" on mobile (dropdown_service.dart). Matched to mobile's wording since that's
// also what LookupService.LOOKUP_FALLBACKS now uses when the API has no seeded rows for this group.
export const MEMBERSHIP_STATUS_MAP: Record<string | number, { label: string, class: string }> = {
    'Applied': { label: 'Pending Approval', class: 'pending' },
    0: { label: 'Pending Approval', class: 'pending' },
    'Active': { label: 'Active', class: 'active' },
    1: { label: 'Active', class: 'active' },
    'InactivePayment': { label: 'Inactive (Unpaid)', class: 'inactive' },
    2: { label: 'Inactive (Unpaid)', class: 'inactive' },
    'InactiveResigned': { label: 'Resigned', class: 'resigned' },
    3: { label: 'Resigned', class: 'resigned' },
    'Terminated': { label: 'Terminated', class: 'terminated' },
    4: { label: 'Terminated', class: 'terminated' }
};

export const MEMBERSHIP_TYPES = [
    'Founding Member',
    'Executive Member',
    'General Member',
    'Associate Member',
    'Honorary Member',
    'Advisory Member',
    'Guest Member'
];

// Membership options moved to grouped section below


export const MEMBER_CATEGORIES = [
    'None',
    'Lifelong Patron',
    'Sponsor',
    'Advisor',
    'Mentor',
    'Recruiter',
    'Active',
    'Volunteer',
    'Contributor',
    'Guest',
    'Student'
];

export const BLOOD_GROUPS = [
    'A+', 'A-', 'B+', 'B-', 'O+', 'O-', 'AB+', 'AB-'
];

// 82.42: not exported any more — getBloodGroupName() below is its only remaining consumer.
// Screens that used to import this for their blood-group dropdown now call
// LookupService.getOptions(LOOKUP_GROUPS.BloodGroup) instead.
const BLOOD_GROUP_OPTIONS = [
    { value: 'Unknown', label: 'Not Specified' },
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

export const TSHIRT_SIZES = [
    { value: 'S', label: 'Small (S)' },
    { value: 'M', label: 'Medium (M)' },
    { value: 'L', label: 'Large (L)' },
    { value: 'XL', label: 'Extra Large (XL)' },
    { value: 'XXL', label: 'Double Extra Large (XXL)' },
    { value: '3XL', label: 'Triple Extra Large (3XL)' }
];

// 82.42: not exported any more — getJobCategoryLabel() below is its only remaining consumer.
// jobs.ts now calls LookupService.getOptions(LOOKUP_GROUPS.JobCategory) to populate its select.
const JOB_CATEGORIES = [
    { id: 'IT', name: 'IT & Software Development' },
    { id: 'Finance', name: 'Finance & Banking' },
    { id: 'Engineering', name: 'Engineering & Construction' },
    { id: 'Marketing', name: 'Marketing & Sales' },
    { id: 'Education', name: 'Education & Research' },
    { id: 'Health', name: 'Healthcare & Pharma' },
    { id: 'PublicSector', name: 'Govt. & Public Sector' },
    { id: 'Mentorship', name: 'Mentorship & Career Guidance' },
    { id: 'Other', name: 'Other Opportunities' }
];

export const ARTICLE_CATEGORIES = [
    { value: 'Event', label: 'Event Highlights' },
    { value: 'Magazine', label: 'E-Magazine Article' },
    { value: 'Regular', label: 'Regular Portal Update' }
];

export const PAYMENT_STATUS_MAP: Record<string | number, { label: string, class: string }> = {
    Pending: { label: 'Pending Verification', class: 'pending' },
    0: { label: 'Pending Verification', class: 'pending' },
    Completed: { label: 'Completed', class: 'success' },
    1: { label: 'Completed', class: 'success' },
    Failed: { label: 'Failed', class: 'failed' },
    2: { label: 'Failed', class: 'failed' },
    Refunded: { label: 'Refunded', class: 'refunded' },
    3: { label: 'Refunded', class: 'refunded' }
};

// TODO 37.3: PledgeStatus { Pledged, PartiallyPaid, Paid, Lapsed, Cancelled } (GHCAA.Domain/Enums.cs).
export const PLEDGE_STATUS_MAP: Record<string | number, { label: string, class: string }> = {
    Pledged: { label: 'Pledged', class: 'pending' },
    0: { label: 'Pledged', class: 'pending' },
    PartiallyPaid: { label: 'Partially Paid', class: 'pending' },
    1: { label: 'Partially Paid', class: 'pending' },
    Paid: { label: 'Paid', class: 'success' },
    2: { label: 'Paid', class: 'success' },
    Lapsed: { label: 'Lapsed', class: 'failed' },
    3: { label: 'Lapsed', class: 'failed' },
    Cancelled: { label: 'Cancelled', class: 'failed' },
    4: { label: 'Cancelled', class: 'failed' }
};

export const SUBMISSION_STATUS = {
    DRAFT: 'Draft',
    PENDING: 'Pending',
    APPROVED: 'Approved',
    REJECTED: 'Rejected'
} as const;

export const SUBMISSION_STATUS_MAP: Record<string | number, { label: string, class: string }> = {
    'Draft': { label: 'Draft', class: 'draft' },
    'Pending': { label: 'Pending Approval', class: 'pending' },
    'Approved': { label: 'Approved', class: 'active' },
    'Rejected': { label: 'Rejected', class: 'terminated' },
    0: { label: 'Draft', class: 'draft' },
    1: { label: 'Pending Approval', class: 'pending' },
    2: { label: 'Approved', class: 'active' },
    3: { label: 'Rejected', class: 'terminated' }
};

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

export function getCurrentECPosition(ecHistory: any[] | undefined): any {
    if (!ecHistory || !Array.isArray(ecHistory) || ecHistory.length === 0) return 'None';
    // Try to find the active one
    const current = ecHistory.find(h => h.isCurrent && !h.endDate);
    if (current) return current.position;

    // Fallback to the latest one if no active found
    return ecHistory[0].position;
}

export function getCurrentECPeriod(ecHistory: any[] | undefined): string {
    if (!ecHistory || !Array.isArray(ecHistory) || ecHistory.length === 0) return '';
    const current = ecHistory.find(h => h.isCurrent && !h.endDate);
    if (current) return current.periodTitle;
    return ecHistory[0].periodTitle;
}

export function getECPositionForPeriod(ecHistory: any[] | undefined, periodId: number | null): any {
    if (!ecHistory || !Array.isArray(ecHistory) || ecHistory.length === 0) return 'None';
    if (!periodId) return getCurrentECPosition(ecHistory);

    // In our ECHistoryDto, we don't have periodId, we have periodTitle.
    // Wait, the ECMember interface HAS ecPeriodId.
    // Let's check what's in the DTO.
    const record = ecHistory.find(h => h.periodId === periodId);
    if (record) return record.position;

    return 'None';
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

export function getArticleCategoryLabel(category: string | number | null | undefined): string {
    if (typeof category === 'number') {
        return ARTICLE_CATEGORIES[category]?.label || 'Article';
    }
    return ARTICLE_CATEGORIES.find(item => item.value === category)?.label || category || 'Article';
}

/**
 * News / Notice filter tabs — shared by the admin console and the public+portal feed so
 * both stay in step. '' means "no filter".
 */
export const POST_TYPE_TABS: { value: '' | 'News' | 'Notice'; label: string }[] = [
    { value: '', label: 'All' },
    { value: 'News', label: 'News' },
    { value: 'Notice', label: 'Notices' }
];

/** Posts predating the PostType column have no value — they count as News. */
export function matchesPostType(
    postType: string | null | undefined,
    filter: '' | 'News' | 'Notice'
): boolean {
    if (!filter) return true;
    return (postType || 'News') === filter;
}

export function getJobCategoryLabel(category: string | null | undefined): string {
    return JOB_CATEGORIES.find(item => item.id === category || item.name === category)?.name || category || 'General';
}

export function getFinancialCategoryLabel(category: string | number | null | undefined): string {
    if (typeof category === 'number') {
        return FINANCIAL_CATEGORY_OPTIONS[category]?.label || 'Other';
    }
    return FINANCIAL_CATEGORY_OPTIONS.find(item => item.value === category)?.label || category || 'Other';
}

export function getPaymentStatusLabel(status: string | number | null | undefined): string {
    if (status === null || status === undefined) return 'Unknown';
    const match = PAYMENT_STATUS_MAP[status];
    if (match) return match.label;
    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return PAYMENT_STATUS_MAP[parseInt(status, 10)]?.label || status;
    }
    return String(status);
}

export function getPaymentStatusClass(status: string | number | null | undefined): string {
    if (status === null || status === undefined) return '';
    const match = PAYMENT_STATUS_MAP[status];
    if (match) return match.class;
    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return PAYMENT_STATUS_MAP[parseInt(status, 10)]?.class || '';
    }
    return '';
}

export function getPledgeStatusLabel(status: string | number | null | undefined): string {
    if (status === null || status === undefined) return 'Unknown';
    const match = PLEDGE_STATUS_MAP[status];
    if (match) return match.label;
    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return PLEDGE_STATUS_MAP[parseInt(status, 10)]?.label || status;
    }
    return String(status);
}

export function getPledgeStatusClass(status: string | number | null | undefined): string {
    if (status === null || status === undefined) return '';
    const match = PLEDGE_STATUS_MAP[status];
    if (match) return match.class;
    if (typeof status === 'string' && /^\d+$/.test(status)) {
        return PLEDGE_STATUS_MAP[parseInt(status, 10)]?.class || '';
    }
    return '';
}

// Work Package 35: the single source for rendering a MembershipType. Accepts either the numeric
// enum ordinal or the enum name, because the API sends both shapes depending on endpoint.
// Never inline a copy of this list in a component — that is exactly how index 6 came to be
// labelled 'Life' in two places while the enum's real member 6 is Guest (see TODO 35.1/35.2).
// Returns the full display label ('Guest Member'), so templates must NOT append ' Member'.
export function getMembershipTypeLabel(type: string | number | null | undefined): string {
    if (type === null || type === undefined || type === '') return 'General';
    if (typeof type === 'number') return MEMBERSHIP_TYPES[type] || 'General';
    if (/^\d+$/.test(type)) return MEMBERSHIP_TYPES[parseInt(type, 10)] || 'General';
    return MEMBERSHIP_TYPE_OPTIONS.find(o => o.value === type)?.label || type;
}

export function getBloodGroupName(bg: string | undefined | null): string {
    if (!bg) return '';
    const option = BLOOD_GROUP_OPTIONS.find(o => o.value === bg);
    return option ? option.label : bg;
}

export const EC_ROLES_OPTIONS = EC_ROLES.map((label, index) => ({ value: index, label }));

// 82.42: MEMBERSHIP_STATUS_OPTIONS and MEMBER_CATEGORY_OPTIONS removed — admin-members.ts and
// directory.ts now get these from LookupService.getOptions(LOOKUP_GROUPS.MembershipStatus /
// .MemberCategory) instead of a local hardcoded copy.

// 62.33: this is getMembershipTypeLabel's fallback source and LOOKUP_FALLBACKS' seed for
// LOOKUP_GROUPS.MembershipType. Screens showing a membership-type picker or filter call
// LookupService.getOptions(LOOKUP_GROUPS.MembershipType) instead of importing this array
// directly, so a different institution's tier labels are a lookups-table change, not a
// code change. This array only stays the values/labels getMembershipTypeLabel() reads
// synchronously and the fallback getOptions() returns if the API has no seeded rows.
export const MEMBERSHIP_TYPE_OPTIONS = [
    { value: 'Founding', label: 'Founding Member' },
    { value: 'Executive', label: 'Executive Member' },
    { value: 'General', label: 'General Member' },
    { value: 'Associate', label: 'Associate Member' },
    { value: 'Honorary', label: 'Honorary Member' },
    { value: 'Advisory', label: 'Advisory Member' },
    { value: 'Guest', label: 'Guest Member' }
];

export const FINANCIAL_CATEGORY_OPTIONS = [
    { value: 'MembershipFee', label: 'Yearly Membership Fee' },
    { value: 'RegistrationFee', label: 'Registration Fee' },
    { value: 'Donation', label: 'Donation' },
    { value: 'Event', label: 'Event Fee' },
    { value: 'Maintenance', label: 'Maintenance' },
    { value: 'Salary', label: 'Salary' },
    { value: 'Utilities', label: 'Utilities' },
    { value: 'ReunionFee', label: 'Reunion Fee' },
    { value: 'Sponsorship', label: 'Sponsorship' },
    { value: 'Grant', label: 'Grant' },
    { value: 'Refund', label: 'Refund' },
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


export const ACADEMIC_SUBJECTS = [
    'None', 'Science', 'Arts & Humanities', 'Business Studies', 'Bengali', 'English', 'History', 'Islamic History & Culture',
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

// 82.42: getAcademicYears() removed — every screen that used it now calls
// LookupService.getAcademicYears() (sourced from the PassingYear lookup group) instead.

export const IS_HSC = (cert: string | undefined | null) => cert === 'HSC';

export const ACADEMIC_DATA = {
    certificates: ACADEMIC_CERTIFICATES,
    subjects: ACADEMIC_SUBJECTS,
    sectors: PROFESSIONAL_SECTORS
};

export const ensureValidAcademicData = (member: any) => {
    // Process AcademicHistory array (New LinkedIn style)
    if (member.academicHistory && Array.isArray(member.academicHistory)) {
        member.academicHistory.forEach((item: any) => {
            if (IS_HSC(item.degree)) {
                // HSC uses science/arts/business from the combined subject list
            }
        });
    }
    if (member.AcademicHistory && Array.isArray(member.AcademicHistory)) {
        member.AcademicHistory.forEach((item: any) => {
            if (IS_HSC(item.degree)) {
                // HSC uses science/arts/business from the combined subject list
            }
        });
    }
};

// Router-navigable paths reused across multiple TS call-sites (router.navigate/parseUrl/
// createUrlTree). Deliberately scoped to paths that appeared duplicated in more than one
// file — not a full route table, and not used from templates (Angular routerLink can't
// import a TS constant without each component re-exposing it, which isn't worth doing for
// a handful of stable, low-churn destinations).
export const ROUTES = {
    LOGIN: '/login',
    PORTAL_DASHBOARD: '/portal/dashboard',
    ADMIN_DASHBOARD: '/admin/dashboard'
};

export const API_ENDPOINTS = {
    ADMIN: {
        MEMBERS: '/api/admin/members',
        MEMBERS_IMPORT: '/api/admin/members/import',
        STATS: '/api/admin/stats',
        COMMUNICATION: '/api/admin/comm',
        GOVERNANCE: '/api/admin/governance',
        CONTACT_MESSAGES: '/api/admin/contact-messages',
        SOCIAL_AUTH: '/api/admin/social-auth',
        POLLS: '/api/admin/polls',
        ERROR_LOGS: '/api/admin/error-logs'
    },
    PENDING: {
        ADMIN_SUMMARY: '/api/pending/admin/summary',
        MY_SUMMARY: '/api/pending/me/summary'
    },
    FAMILY_LINKS: '/api/family-links',
    MENTORSHIP: '/api/mentorship',
    AUTH: {
        LOGIN: '/api/auth/login',
        REGISTER: '/api/auth/register',
        VERIFY_EMAIL: '/api/auth/verify-email',
        RESEND_OTP: '/api/auth/resend-otp',
        STATUS: '/api/auth/status',
        PROVIDERS: '/api/auth/providers',
        // 7.13: admin step-up (re-verify by emailed OTP before destructive/financial actions)
        STEP_UP_REQUEST: '/api/auth/admin/step-up/request',
        STEP_UP_VERIFY: '/api/auth/admin/step-up/verify',
        GOOGLE: '/api/auth/google',
        FACEBOOK: '/api/auth/facebook',
        REFRESH: '/api/auth/refresh',
        ME: '/api/auth/me',
        LOGOUT: '/api/auth/logout',
        RESET_PASSWORD: '/api/auth/reset-password'
    },
    ROLES: '/api/roles',
    ACTIVITY: {
        ADMIN_GLOBAL: '/api/activity/admin/global'
    },
    EVENTS: '/api/events',
    GALLERY: '/api/gallery',
    NEWS: '/api/news',
    SITE_CONTENT: '/api/site-content',
    JOBS: '/api/jobs',
    PROFILE: '/api/profile',
    FINANCIALS: '/api/financials',
    CAMPAIGNS: '/api/campaigns',
    MESSAGING: {
        RECENT: '/api/messaging/recent',
        HISTORY: '/api/messaging/history',
        SEND: '/api/messaging/send'
    },
    HUBS: {
        CHAT: '/api/hubs/chat',
        NOTIFICATIONS: '/api/hubs/notifications'
    },
    NOTIFICATIONS: {
        BASE: '/api/notifications',
        READ_ALL: '/api/notifications/read-all'
    },
    NETWORKING: {
        BASE: '/api/networking',
        COMMITTEE: '/api/networking/committee',
        COMMITTEE_PERIODS: '/api/networking/periods',
        SEARCH: '/api/networking/search',
        UPDATES: '/api/networking/updates'
    },
    // Public governance surface. Distinct from ADMIN.GOVERNANCE ('/api/admin/governance') —
    // these three are [AllowAnonymous] on GovernanceController; VOTE is member-authenticated.
    GOVERNANCE: {
        CONSTITUTION: '/api/governance/constitution',
        CONSTITUTION_HISTORY: '/api/governance/constitution/history',
        CONSTITUTION_VOTE: (id: number) => `/api/governance/constitution/${id}/vote`
    },
    LOOKUPS: '/api/lookups',
    LEDGER: '/api/ledger',
    CONTACT: '/api/contact',
    ASSISTANT: '/api/assistant/ask',
    THEMES: '/api/theme',
    SECURE_FILES: '/api/secure-files',
    PAYMENT_CONFIG: {
        BASE: '/api/payment-config',
        PUBLIC: '/api/payment-config/active'
    },
    POLLS: {
        BASE: '/api/polls',
        ACTIVE: '/api/polls/active'
    },
    CONFIG: '/api/config',
    FORUM: '/api/forum',
    HEALTH: '/healthz'
} as const;
