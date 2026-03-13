import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/public-layout/public-layout').then(m => m.PublicLayout),
        children: [
            {
                path: '',
                loadComponent: () => import('./features/landing/landing').then(m => m.Landing)
            },
            {
                path: 'login',
                loadComponent: () => import('./features/login/login').then(m => m.Login)
            },
            {
                path: 'register',
                loadComponent: () => import('./features/register/register').then(m => m.Register)
            },
            {
                path: 'about',
                loadComponent: () => import('./features/about/about').then(m => m.About)
            },
            {
                path: 'contact',
                loadComponent: () => import('./features/contact/contact').then(m => m.Contact)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./features/gallery/gallery').then(m => m.Gallery)
            },
            {
                path: 'events',
                loadComponent: () => import('./features/events/events').then(m => m.Events)
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
                loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard)
            },
            {
                path: 'jobs',
                loadComponent: () => import('./features/jobs/jobs').then(m => m.Jobs)
            },
            {
                path: 'id-card',
                loadComponent: () => import('./features/digital-id/digital-id').then(m => m.DigitalId)
            },
            {
                path: 'payments',
                loadComponent: () => import('./features/payments/payments').then(m => m.Payments)
            },
            {
                path: 'profile',
                loadComponent: () => import('./features/profile/profile').then(m => m.Profile)
            },
            {
                path: 'directory',
                loadComponent: () => import('./features/directory/directory').then(m => m.Directory)
            },
            {
                path: 'governance',
                loadComponent: () => import('./features/governance/governance').then(m => m.Governance)
            },
            {
                path: 'messages',
                loadComponent: () => import('./features/messages/messages').then(m => m.Messages)
            },
            {
                path: 'assistant',
                loadComponent: () => import('./features/assistant/assistant').then(m => m.Assistant)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./features/gallery/gallery').then(m => m.Gallery)
            },
            {
                path: 'news',
                loadComponent: () => import('./features/news/news').then(m => m.News)
            },
            {
                path: 'events',
                loadComponent: () => import('./features/events/events').then(m => m.Events)
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
                loadComponent: () => import('./features/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard)
            },
            {
                path: 'approvals',
                loadComponent: () => import('./features/member-approval/member-approval').then(m => m.MemberApproval)
            },
            {
                path: 'members',
                loadComponent: () => import('./features/admin-members/admin-members').then(m => m.AdminMembers)
            },
            {
                path: 'members/ec',
                loadComponent: () => import('./features/admin-governance/admin-governance').then(m => m.AdminGovernance)
            },
            {
                path: 'news',
                loadComponent: () => import('./features/admin-news/admin-news').then(m => m.AdminNews)
            },
            {
                path: 'gallery',
                loadComponent: () => import('./features/admin-gallery/admin-gallery').then(m => m.AdminGallery)
            },
            {
                path: 'comm',
                loadComponent: () => import('./features/admin-comm/admin-comm').then(m => m.AdminComm)
            },
            {
                path: 'ledger',
                loadComponent: () => import('./features/ledger/ledger').then(m => m.Ledger)
            },
            {
                path: 'events',
                loadComponent: () => import('./features/admin-events/admin-events').then(m => m.AdminEvents)
            },
            {
                path: 'themes',
                loadComponent: () => import('./features/admin-themes/admin-themes').then(m => m.AdminThemes)
            },
            {
                path: 'payments',
                loadComponent: () => import('./features/admin-payment-config/admin-payment-config').then(m => m.AdminPaymentConfig)
            },
            {
                path: 'roles',
                loadComponent: () => import('./features/admin-roles/admin-roles').then(m => m.AdminRoles)
            },
            {
                path: 'audit',
                loadComponent: () => import('./features/admin-audit/admin-audit').then(m => m.AdminAudit)
            }
        ]
    }
];
