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
