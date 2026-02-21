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
            }
        ]
    },
    {
        path: 'admin',
        loadComponent: () => import('./layouts/admin-layout/admin-layout').then(m => m.AdminLayout),
        canActivate: [authGuard, adminGuard],
        children: [
            { path: '', redirectTo: 'approvals', pathMatch: 'full' },
            {
                path: 'approvals',
                loadComponent: () => import('./features/member-approval/member-approval').then(m => m.MemberApproval)
            },
            {
                path: 'ledger',
                loadComponent: () => import('./features/ledger/ledger').then(m => m.Ledger)
            }
        ]
    }
];
