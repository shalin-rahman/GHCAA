import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ThemeService, SpecialDayTheme } from '../../core/services/theme.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { toWireDate, toDisplayDate, parseDisplayDate } from '../../core/utils/date.util';
import { OrgConfigService } from '../../core/services/org-config.service';

export type ThemeStatus = 'SCHEDULED' | 'LIVE' | 'EXPIRED' | 'IDLE';

@Component({
    selector: 'app-admin-themes',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent],
    templateUrl: './admin-themes.html',
    styleUrl: './admin-themes.scss'
})
export class AdminThemes implements OnInit {
    private adminService = inject(AdminService);
    private themeService = inject(ThemeService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    orgConfig = inject(OrgConfigService);

    themes = signal<SpecialDayTheme[]>([]);
    loading = signal(true);
    showForm = signal(false);
    isEditing = signal(false);
    isSaving = signal(false);

    formatDateToDMY(d: any) {
        return toDisplayDate(d);
    }

    /** SCHEDULED / LIVE / EXPIRED / IDLE — 30.17: the LIVE badge previously just mirrored
     *  `isEnabled`, ignoring the start/end date window entirely (an expired range still
     *  showed LIVE). Disabled themes are always IDLE; enabled themes are resolved against
     *  today's date vs. the theme's own start/end window. */
    themeStatus(theme: SpecialDayTheme): ThemeStatus {
        if (!theme.isEnabled) return 'IDLE';

        const start = parseDisplayDate(theme.startDate);
        const end = parseDisplayDate(theme.endDate);
        if (!start || !end) return 'IDLE';

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        start.setHours(0, 0, 0, 0);
        end.setHours(0, 0, 0, 0);

        if (today < start) return 'SCHEDULED';
        if (today > end) return 'EXPIRED';
        return 'LIVE';
    }

    selectedTheme: SpecialDayTheme = this.resetTheme();

    ngOnInit() {
        this.loadThemes();
    }

    loadThemes() {
        this.loading.set(true);
        this.adminService.getAllThemes().subscribe({
            next: (data) => {
                this.themes.set(data);
                this.loading.set(false);
                // Refresh active theme globally
                this.themeService.loadActiveSpecialTheme();
            },
            error: () => this.loading.set(false)
        });
    }

    openCreate() {
        this.isEditing.set(false);
        this.selectedTheme = this.resetTheme();
        this.showForm.set(true);
    }

    openEdit(theme: SpecialDayTheme) {
        this.isEditing.set(true);
        this.selectedTheme = { 
            ...theme,
            startDate: this.formatDateToDMY(theme.startDate),
            endDate: this.formatDateToDMY(theme.endDate)
        };
        this.showForm.set(true);
    }

    saveTheme() {
        if (!this.selectedTheme.title || !this.selectedTheme.startDate || !this.selectedTheme.endDate) {
            this.notify.warning('Title, Start Date and End Date are mandatory assets.');
            return;
        }

        if (new Date(this.selectedTheme.endDate) < new Date(this.selectedTheme.startDate)) {
            this.notify.warning('Theme end date must be after or on the start date.');
            return;
        }

        this.isSaving.set(true);
        const payload = {
            ...this.selectedTheme,
            startDate: toWireDate(this.selectedTheme.startDate),
            endDate: toWireDate(this.selectedTheme.endDate)
        };
        const obs = this.isEditing()
            ? this.adminService.updateTheme(this.selectedTheme.id, payload)
            : this.adminService.createTheme(payload);

        obs.subscribe({
            next: () => {
                this.notify.success(this.isEditing() ? 'Theme updated' : 'Theme created');
                this.showForm.set(false);
                this.isSaving.set(false);
                this.loadThemes();
            },
            error: () => {
                this.notify.error('Failed to save theme');
                this.isSaving.set(false);
            }
        });
    }

    async deleteTheme(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete theme',
            message: 'Are you sure you want to delete this theme?',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.adminService.deleteTheme(id).subscribe({
            next: () => {
                this.notify.success('Theme deleted');
                this.loadThemes();
            },
            // 29F.2: surface HTTP failures instead of failing silently
            error: () => this.notify.error('Failed to delete theme.')
        });
    }



    private resetTheme(): SpecialDayTheme {
        return {
            id: 0,
            title: '',
            startDate: this.formatDateToDMY(new Date()),
            endDate: this.formatDateToDMY(new Date()),
            backgroundColor: '#000000',
            textColor: '#ffffff',
            announcementText: '',
            animationStyle: 'None',
            imageUrl: '',
            sidebarColor: '#111111',
            enableGradientFading: false,
            isActive: false,
            isEnabled: true
        };
    }
}


