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
                title: 'Govt. Haraganga College Alumni Association | Haragangian Portal',
                data: { description: 'Official alumni association of Govt. Haraganga College, Munshiganj — reconnecting Haragangians worldwide through heritage, networking, and advancement.' }
            },
            {
                path: 'login',
                loadComponent: () => import('./public/login/login').then(m => m.Login),
                title: 'Member Login | GHCAA'
            },
            {
                path: 'register',
                loadComponent: () => import('./public/register/register').then(m => m.Register),
                title: 'Join the Association | GHCAA',
                data: { description: 'Register as a member of the Govt. Haraganga College Alumni Association (GHCAA), Munshiganj.' }
            },
            {
                path: 'reset-password',
                loadComponent: () => import('./public/reset-password/reset-password').then(m => m.ResetPassword),
                title: 'Reset Password | GHCAA'
            },
            {
                path: 'about',
                loadComponent: () => import('./public/about/about').then(m => m.About),
                title: 'About Us | Govt. Haraganga College Alumni Association',
                data: { description: 'The history, mission, and legacy of Govt. Haraganga College, Munshiganj and its Alumni Association.' }
            },
            {
                path: 'contact',
                loadComponent: () => import('./public/contact/contact').then(m => m.Contact),
                title: 'Contact Us | GHCAA',
                data: { description: 'Get in touch with the Govt. Haraganga College Alumni Association secretariat.' }
            },
            {
                // Public governance surface. Both pages are anonymous by design — the
                // constitution and the election rules must be readable without a login.
                path: 'constitution',
                loadComponent: () => import('./public/constitution/constitution').then(m => m.ConstitutionPage),
                title: 'Constitution | GHCAA'
            },
            {
                path: 'elections',
                loadComponent: () => import('./public/elections/elections').then(m => m.ElectionsPage),
                title: 'Elections & Governance | GHCAA'
            },
            {
                path: 'gallery',
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery),
                canActivate: [featureGuard('enableGallery')],
                title: 'Event Gallery | GHCAA'
            },
            {
                path: 'magazine',
                loadComponent: () => import('./public/magazine/magazine').then(m => m.Magazine),
                title: 'Alumni Magazine | GHCAA'
            },
            {
                path: 'directory',
                loadComponent: () => import('./public/directory/public-directory').then(m => m.PublicDirectory),
                title: 'Alumni Directory | GHCAA'
            },
            {
                path: 'events',
                canActivate: [featureGuard('enableEvents')],
                title: 'Events | GHCAA',
                data: { description: 'Upcoming and past events hosted by the Govt. Haraganga College Alumni Association, Munshiganj.' },
                children: [
                    { path: '', loadComponent: () => import('./common/events/events').then(m => m.Events) },
                    { path: ':id', loadComponent: () => import('./common/events/events').then(m => m.Events) }
                ]
            },
            {
                path: 'news',
                loadComponent: () => import('./common/news/news').then(m => m.News),
                title: 'News & Notices | GHCAA',
                data: { description: 'Latest news, updates, and notices from Govt. Haraganga College Alumni Association, Munshiganj.' }
            },
            {
                path: 'jobs',
                loadComponent: () => import('./common/jobs/jobs').then(m => m.Jobs),
                canActivate: [featureGuard('enableJobHub')],
                title: 'Job Hub | GHCAA'
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
                path: 'assistant',
                loadComponent: () => import('./member/assistant/assistant').then(m => m.Assistant)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery),
                canActivate: [featureGuard('enableGallery')]
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
                path: 'polls',
                loadComponent: () => import('./member/polls/polls.component').then(m => m.MemberPolls)
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
