import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService, SpecialDayTheme } from '../../core/services/theme.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';

@Component({
    selector: 'app-admin-themes',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent],
    templateUrl: './admin-themes.html',
    styleUrl: './admin-themes.scss'
})
export class AdminThemes implements OnInit {
    private adminService = inject(AdminService);
    private themeService = inject(ThemeService);
    private notify = inject(NotificationService);

    themes = signal<SpecialDayTheme[]>([]);
    loading = signal(true);
    showForm = signal(false);
    isEditing = signal(false);
    isSaving = signal(false);

    formatDateToDMY(d: any) {
        return toDisplayDate(d);
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

    deleteTheme(id: number) {
        if (!confirm('Are you sure you want to delete this theme?')) return;
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


