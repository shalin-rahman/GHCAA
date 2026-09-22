import { Routes } from '@angular/router';
import { authGuard, adminGuard, superAdminGuard, memberGuard } from './core/guards/auth.guard';
import { featureGuard } from './core/guards/feature.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/public-layout/public-layout').then(m => m.PublicLayout),
        children: [
            {
                path: '',
                loadComponent: () => import('./public/landing/landing').then(m => m.Landing),
                title: '{branding.fullName} | {branding.memberNickname} Portal',
                data: { description: 'Official alumni association of {branding.institutionName}, Munshiganj — reconnecting {branding.memberNickname}s worldwide through heritage, networking, and advancement.' }
            },
            {
                path: 'login',
                loadComponent: () => import('./public/login/login').then(m => m.Login),
                title: 'Member Login | {branding.shortName}'
            },
            {
                path: 'register',
                loadComponent: () => import('./public/register/register').then(m => m.Register),
                title: 'Join the Association | {branding.shortName}',
                data: { description: 'Register as a member of {branding.fullName} ({branding.shortName}), Munshiganj.' }
            },
            {
                path: 'reset-password',
                loadComponent: () => import('./public/reset-password/reset-password').then(m => m.ResetPassword),
                title: 'Reset Password | {branding.shortName}'
            },
            {
                path: 'about',
                loadComponent: () => import('./public/about/about').then(m => m.About),
                title: 'About Us | {branding.fullName}',
                data: { description: 'The history, mission, and legacy of {branding.institutionName}, Munshiganj and its Alumni Association.' }
            },
            {
                path: 'contact',
                loadComponent: () => import('./public/contact/contact').then(m => m.Contact),
                title: 'Contact Us | {branding.shortName}',
                data: { description: 'Get in touch with the {branding.fullName} secretariat.' }
            },
            {
                // Public governance surface. Both pages are anonymous by design — the
                // constitution and the election rules must be readable without a login.
                path: 'constitution',
                loadComponent: () => import('./public/constitution/constitution').then(m => m.ConstitutionPage),
                title: 'Constitution | {branding.shortName}'
            },
            {
                path: 'elections',
                loadComponent: () => import('./public/elections/elections').then(m => m.ElectionsPage),
                title: 'Elections & Governance | {branding.shortName}'
            },
            {
                path: 'elections/results',
                loadComponent: () => import('./public/elections/election-results').then(m => m.ElectionResults),
                title: 'Election Results | {branding.shortName}'
            },
            {
                path: 'gallery',
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery),
                canActivate: [featureGuard('enableGallery')],
                title: 'Event Gallery | {branding.shortName}'
            },
            {
                path: 'magazine',
                loadComponent: () => import('./public/magazine/magazine').then(m => m.Magazine),
                title: 'Alumni Magazine | {branding.shortName}'
            },
            {
                path: 'directory',
                loadComponent: () => import('./public/directory/public-directory').then(m => m.PublicDirectory),
                title: 'Alumni Directory | {branding.shortName}'
            },
            {
                path: 'events',
                canActivate: [featureGuard('enableEvents')],
                title: 'Events | {branding.shortName}',
                data: { description: 'Upcoming and past events hosted by {branding.fullName}, Munshiganj.' },
                children: [
                    { path: '', loadComponent: () => import('./common/events/events').then(m => m.Events) },
                    { path: ':id', loadComponent: () => import('./common/events/events').then(m => m.Events) }
                ]
            },
            {
                // TODO 37.3: fundraising campaigns + donor honour roll.
                path: 'campaigns',
                canActivate: [featureGuard('enableFundraising')],
                title: 'Fundraising Campaigns | {branding.shortName}',
                data: { description: 'Support {branding.fullName} through an active fundraising campaign.' },
                children: [
                    { path: '', loadComponent: () => import('./public/campaigns/campaigns').then(m => m.Campaigns) },
                    { path: ':slug', loadComponent: () => import('./public/campaigns/campaigns').then(m => m.Campaigns) }
                ]
            },
            {
                path: 'scholarships',
                loadComponent: () => import('./public/scholarships/scholarships').then(m => m.Scholarships),
                canActivate: [featureGuard('enableScholarships')],
                title: 'Scholarships | {branding.shortName}',
                data: { description: 'Apply for scholarships and check an application status.' }
            },
            {
                path: 'verify',
                children: [
                    { path: '', loadComponent: () => import('./public/verify/verify').then(m => m.Verify) },
                    { path: ':shortCode', loadComponent: () => import('./public/verify/verify').then(m => m.Verify) }
                ]
            },
            {
                path: 'legacy',
                canActivate: [featureGuard('enableLegacyArchive')],
                title: 'Oral History Archive | {branding.shortName}',
                data: { description: 'Approved oral histories and community memories from {branding.fullName}.' },
                children: [
                    { path: '', loadComponent: () => import('./public/legacy/legacy').then(m => m.LegacyPage) },
                    { path: 'item/:id', loadComponent: () => import('./public/legacy/legacy').then(m => m.LegacyPage) }
                ]
            },
            {
                path: 'news',
                loadComponent: () => import('./common/news/news').then(m => m.News),
                title: 'News & Notices | {branding.shortName}',
                data: { description: 'Latest news, updates, and notices from {branding.fullName}, Munshiganj.' }
            },
            {
                path: 'jobs',
                loadComponent: () => import('./common/jobs/jobs').then(m => m.Jobs),
                canActivate: [featureGuard('enableJobHub')],
                title: 'Job Hub | {branding.shortName}'
            },
            {
                path: 'payment',
                children: [
                    { path: 'success', loadComponent: () => import('./common/payment-status/payment-status').then(m => m.PaymentStatus) },
                    { path: 'failed', loadComponent: () => import('./common/payment-status/payment-status').then(m => m.PaymentStatus) }
                ]
            },
            {
                path: 'healthz',
                loadComponent: () => import('./common/health/health').then(m => m.Health)
            }
        ]
    },
    {
        path: 'portal',
        loadComponent: () => import('./layouts/portal-layout/portal-layout').then(m => m.PortalLayout),
        canActivate: [authGuard, memberGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            {
                // 29A.1: mustChangePassword users are redirected here by authGuard; the route must exist
                // (previously 404'd → hard login lockout).
                path: 'change-password',
                loadComponent: () => import('./member/change-password/change-password').then(m => m.ChangePassword)
            },
            {
                path: 'dashboard',
                loadComponent: () => import('./member/dashboard/dashboard').then(m => m.Dashboard)
            },
            {
                path: 'jobs',
                loadComponent: () => import('./common/jobs/jobs').then(m => m.Jobs),
                canActivate: [featureGuard('enableJobHub')]
            },
            {
                path: 'id-card',
                loadComponent: () => import('./member/digital-id/digital-id').then(m => m.DigitalId)
            },
            {
                path: 'payments',
                loadComponent: () => import('./member/payments/payments').then(m => m.Payments)
            },
            {
                path: 'profile',
                loadComponent: () => import('./member/profile/profile').then(m => m.Profile)
            },
            {
                path: 'directory',
                loadComponent: () => import('./common/directory/directory').then(m => m.Directory)
            },
            {
                path: 'governance',
                loadComponent: () => import('./common/governance/governance').then(m => m.Governance)
            },
            {
                path: 'messages',
                loadComponent: () => import('./member/messages/messages').then(m => m.Messages)
            },
            {
                path: 'communications',
                loadComponent: () => import('./member/communications/communications').then(m => m.MemberCommunications)
            },
            {
                path: 'assistant',
                loadComponent: () => import('./member/assistant/assistant').then(m => m.Assistant)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery),
                canActivate: [featureGuard('enableGallery')]
            },
            {
                // TODO 37.3: member's own pledges + giving history.
                path: 'giving',
                loadComponent: () => import('./member/giving/giving').then(m => m.Giving),
                canActivate: [featureGuard('enableFundraising')]
            },
            {
                path: 'news',
                loadComponent: () => import('./common/news/news').then(m => m.News)
            },
            {
                path: 'events',
                canActivate: [featureGuard('enableEvents')],
                children: [
                    { path: '', loadComponent: () => import('./common/events/events').then(m => m.Events) },
                    { path: ':id', loadComponent: () => import('./common/events/events').then(m => m.Events) }
                ]
            },
            {
                path: 'articles',
                loadComponent: () => import('./member/articles/articles').then(m => m.MemberArticles)
            },
            {
                path: 'requests',
                loadComponent: () => import('./member/requests/requests').then(m => m.MemberRequests)
            },
            {
                path: 'polls',
                loadComponent: () => import('./member/polls/polls.component').then(m => m.MemberPolls)
            },
            {
                path: 'election',
                loadComponent: () => import('./member/election/election').then(m => m.MemberElection)
            },
            {
                path: 'forum',
                loadComponent: () => import('./member/forum/forum').then(m => m.Forum),
                canActivate: [featureGuard('enableForum')]
            },
            {
                path: 'forum/:id',
                loadComponent: () => import('./member/forum/topic-detail').then(m => m.TopicDetail),
                canActivate: [featureGuard('enableForum')]
            }
        ]
    },
    {
        path: 'admin',
        loadComponent: () => import('./layouts/admin-layout/admin-layout').then(m => m.AdminLayout),
        canActivate: [authGuard, adminGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            {
                path: 'dashboard',
                loadComponent: () => import('./admin/dashboard/admin-dashboard').then(m => m.AdminDashboard)
            },
            {
                path: 'approvals',
                loadComponent: () => import('./admin/member-approval/member-approval').then(m => m.MemberApproval)
            },
            {
                path: 'members',
                loadComponent: () => import('./admin/members/admin-members').then(m => m.AdminMembers)
            },
            {
                path: 'members/ec',
                loadComponent: () => import('./admin/governance/admin-governance').then(m => m.AdminGovernance)
            },
            {
                path: 'news',
                loadComponent: () => import('./admin/news/admin-news').then(m => m.AdminNews)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./admin/gallery/admin-gallery').then(m => m.AdminGallery)
            },
            {
                // TODO 37.3: create/edit campaigns, confirm pledge receipts, manage donor tiers.
                path: 'campaigns',
                loadComponent: () => import('./admin/campaigns/admin-campaigns').then(m => m.AdminCampaigns)
            },
            {
                path: 'comm',
                loadComponent: () => import('./admin/comm/admin-comm').then(m => m.AdminComm)
            },
            {
                path: 'ledger',
                loadComponent: () => import('./admin/ledger/ledger').then(m => m.Ledger),
                canActivate: [superAdminGuard]
            },
            {
                path: 'events',
                loadComponent: () => import('./admin/events/admin-events').then(m => m.AdminEvents)
            },
            {
                path: 'themes',
                loadComponent: () => import('./admin/themes/admin-themes').then(m => m.AdminThemes)
            },
            {
                path: 'payments/fees',
                loadComponent: () => import('./admin/fee-config/admin-fee-config').then(m => m.AdminFeeConfig),
                canActivate: [superAdminGuard]
            },
            {
                path: 'payments',
                loadComponent: () => import('./admin/payment-config/admin-payment-config').then(m => m.AdminPaymentConfig),
                canActivate: [superAdminGuard]
            },
            {
                path: 'roles',
                loadComponent: () => import('./admin/roles/admin-roles').then(m => m.AdminRoles),
                canActivate: [superAdminGuard]
            },
            {
                path: 'audit',
                loadComponent: () => import('./admin/audit/admin-audit').then(m => m.AdminAudit),
                canActivate: [superAdminGuard]
            },
            {
                path: 'error-logs',
                loadComponent: () => import('./admin/error-logs/admin-error-logs').then(m => m.AdminErrorLogs),
                canActivate: [superAdminGuard]
            },
            {
                path: 'dev-tracker',
                loadComponent: () => import('./admin/dev-tracker/admin-dev-tracker').then(m => m.AdminDevTracker),
                canActivate: [superAdminGuard]
            },
            {
                path: 'article-approvals',
                loadComponent: () => import('./admin/article-approval/article-approval').then(m => m.ArticleApproval)
            },
            {
                path: 'gallery-approvals',
                loadComponent: () => import('./admin/gallery-approval/gallery-approval').then(m => m.GalleryApproval)
            },
            {
                path: 'job-approvals',
                loadComponent: () => import('./admin/job-approval/job-approval').then(m => m.JobApproval)
            },
            {
                path: 'contact-messages',
                loadComponent: () => import('./admin/contact-messages/contact-messages').then(m => m.ContactMessages)
            },
            {
                path: 'polls',
                loadComponent: () => import('./admin/polls/polls.component').then(m => m.AdminPolls)
            },
            {
                path: 'elections',
                loadComponent: () => import('./admin/elections/admin-elections').then(m => m.AdminElections)
            },
            {
                path: 'site-content',
                loadComponent: () => import('./admin/site-content/site-content').then(m => m.AdminSiteContent)
            },
            {
                path: 'org-config',
                loadComponent: () => import('./admin/org-config/org-config').then(m => m.AdminOrgConfig),
                canActivate: [superAdminGuard]
            }
        ]
    }
];
