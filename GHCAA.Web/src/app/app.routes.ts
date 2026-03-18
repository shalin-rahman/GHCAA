import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/public-layout/public-layout').then(m => m.PublicLayout),
        children: [
            {
                path: '',
                loadComponent: () => import('./public/landing/landing').then(m => m.Landing)
            },
            {
                path: 'login',
                loadComponent: () => import('./public/login/login').then(m => m.Login)
            },
            {
                path: 'register',
                loadComponent: () => import('./public/register/register').then(m => m.Register)
            },
            {
                path: 'reset-password',
                loadComponent: () => import('./public/reset-password/reset-password').then(m => m.ResetPassword)
            },
            {
                path: 'about',
                loadComponent: () => import('./public/about/about').then(m => m.About)
            },
            {
                path: 'contact',
                loadComponent: () => import('./public/contact/contact').then(m => m.Contact)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery)
            },
            {
                path: 'magazine',
                loadComponent: () => import('./public/magazine/magazine').then(m => m.Magazine)
            },
            {
                path: 'directory',
                loadComponent: () => import('./public/directory/public-directory').then(m => m.PublicDirectory)
            },
            {
                path: 'events',
                children: [
                    { path: '', loadComponent: () => import('./common/events/events').then(m => m.Events) },
                    { path: ':id', loadComponent: () => import('./common/events/events').then(m => m.Events) }
                ]
            },
            {
                path: 'news',
                loadComponent: () => import('./common/news/news').then(m => m.News)
            },
            {
                path: 'jobs',
                loadComponent: () => import('./common/jobs/jobs').then(m => m.Jobs)
            },
            {
                path: 'payment',
                children: [
                    { path: 'success', loadComponent: () => import('./common/payment-status/payment-status').then(m => m.PaymentStatus) },
                    { path: 'failed', loadComponent: () => import('./common/payment-status/payment-status').then(m => m.PaymentStatus) }
                ]
            }
        ]
    },
    {
        path: 'portal',
        loadComponent: () => import('./layouts/portal-layout/portal-layout').then(m => m.PortalLayout),
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            {
                path: 'dashboard',
                loadComponent: () => import('./member/dashboard/dashboard').then(m => m.Dashboard)
            },
            {
                path: 'jobs',
                loadComponent: () => import('./common/jobs/jobs').then(m => m.Jobs)
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
                loadComponent: () => import('./common/gallery/gallery').then(m => m.Gallery)
            },
            {
                path: 'news',
                loadComponent: () => import('./common/news/news').then(m => m.News)
            },
            {
                path: 'events',
                children: [
                    { path: '', loadComponent: () => import('./common/events/events').then(m => m.Events) },
                    { path: ':id', loadComponent: () => import('./common/events/events').then(m => m.Events) }
                ]
            },
            {
                path: 'articles',
                loadComponent: () => import('./member/articles/articles').then(m => m.MemberArticles)
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
                loadComponent: () => import('./admin/ledger/ledger').then(m => m.Ledger)
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
                path: 'payments',
                loadComponent: () => import('./admin/payment-config/admin-payment-config').then(m => m.AdminPaymentConfig)
            },
            {
                path: 'roles',
                loadComponent: () => import('./admin/roles/admin-roles').then(m => m.AdminRoles)
            },
            {
                path: 'audit',
                loadComponent: () => import('./admin/audit/admin-audit').then(m => m.AdminAudit)
            },
            {
                path: 'article-approvals',
                loadComponent: () => import('./admin/article-approval/article-approval').then(m => m.ArticleApproval)
            },
            {
                path: 'contact-messages',
                loadComponent: () => import('./admin/contact-messages/contact-messages').then(m => m.ContactMessages)
            }
        ]
    }
];
