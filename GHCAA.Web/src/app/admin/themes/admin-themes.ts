import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ThemeService, SpecialDayTheme } from '../../core/services/theme.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { toWireDate, toDisplayDate, parseDisplayDate } from '../../core/utils/date.util';
import { OrgConfigService } from '../../core/services/org-config.service';
import { AppDateInputComponent } from '../../common/date-input/date-input.component';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import {
    adjustLightness,
    MIN_TEXT_CONTRAST,
    parseHexColor,
    rgbToHex,
    safeContrastRatio,
    type Rgb
} from '../../core/utils/color-math';

export type ThemeStatus = 'SCHEDULED' | 'LIVE' | 'EXPIRED' | 'IDLE';
type BannerSample = { position: number; color: string };
type ContrastAdvice =
    | { status: 'invalid'; message: string }
    | { status: 'pass' | 'warning'; message: string; ratio: number; position: number; backgroundColor: string };

@Component({
    selector: 'app-admin-themes',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, PageHeaderComponent, AppDateInputComponent, ImgFallbackDirective],
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
        return toDisplayDate(d, this.orgConfig.dateFormat());
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
            // The global interceptor already toasts the server's reason, such as a duplicate
            // title and start date (62.56g).
            error: () => this.isSaving.set(false)
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

    previewBackground(): string {
        const { backgroundColor, sidebarColor, enableGradientFading } = this.selectedTheme;
        return enableGradientFading
            ? `linear-gradient(135deg, ${backgroundColor}, ${sidebarColor || backgroundColor})`
            : backgroundColor;
    }

    // The template calls this on every change detection, and a gradient check builds up to 256
    // samples, so the result is kept until one of the inputs it reads changes.
    private adviceCache: { key: string; advice: ContrastAdvice | null } | null = null;

    contrastAdvice(): ContrastAdvice | null {
        const { imageUrl, backgroundColor, sidebarColor, enableGradientFading, textColor } = this.selectedTheme;
        const key = JSON.stringify([imageUrl, backgroundColor, sidebarColor, enableGradientFading, textColor]);
        if (this.adviceCache?.key !== key) {
            this.adviceCache = { key, advice: this.computeContrastAdvice() };
        }
        return this.adviceCache.advice;
    }

    private computeContrastAdvice(): ContrastAdvice | null {
        const theme = this.selectedTheme;
        if (theme.imageUrl) return null;

        const samples = this.bannerSamples();
        if (!samples || !parseHexColor(theme.textColor)) {
            return { status: 'invalid', message: 'Enter a hex color like #1a2b3c.' };
        }

        const worst = this.worstContrast(theme.textColor, samples);
        const ratio = worst.ratio;
        const location = theme.enableGradientFading ? ` at ${worst.position}% of the gradient` : '';
        const status = ratio < MIN_TEXT_CONTRAST ? 'warning' : 'pass';
        // Rounded down so a failing 4.496 shows as 4.49, not as a passing-looking 4.50.
        // The small epsilon stops float error from turning 1.15 into 1.14.
        const shown = (Math.floor(ratio * 100 + 1e-9) / 100).toFixed(2);
        const message = status === 'warning'
            ? `Text contrast is ${shown}:1${location}; it should be at least ${MIN_TEXT_CONTRAST}:1.`
            : `Text contrast is ${shown}:1${location}.`;

        return { status, message, ratio, position: worst.position, backgroundColor: worst.color };
    }

    useSuggestedTextColor(): void {
        const advice = this.contrastAdvice();
        if (!advice || advice.status !== 'warning') return;

        const suggestion = this.suggestTextColor(this.selectedTheme.textColor, this.bannerSamples() ?? []);
        if (!suggestion) {
            this.notify.warning('No text color reaches 4.5:1 on this banner. Change the banner colors instead.');
            return;
        }
        this.selectedTheme.textColor = suggestion;
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

    // Every banner color the text can sit on, or null when a color the live banner uses is not
    // valid hex. The live banner ignores sidebarColor without the gradient and falls back to
    // backgroundColor when it is empty, so neither case counts as invalid here.
    private bannerSamples(): BannerSample[] | null {
        const { backgroundColor, sidebarColor, enableGradientFading } = this.selectedTheme;
        const start = parseHexColor(backgroundColor);
        if (!start) return null;
        if (!enableGradientFading || !sidebarColor?.trim()) {
            return [{ color: rgbToHex(start), position: 0 }];
        }
        const end = parseHexColor(sidebarColor);
        return end ? this.sampleGradient(start, end) : null;
    }

    // One step per change in the largest channel visits every 8-bit color the gradient can
    // render, so a low point between fixed percentages cannot be missed.
    private sampleGradient(start: Rgb, end: Rgb): BannerSample[] {
        const steps = Math.max(Math.abs(end.r - start.r), Math.abs(end.g - start.g), Math.abs(end.b - start.b), 1);
        return Array.from({ length: steps + 1 }, (_, i) => {
            const progress = i / steps;
            return {
                position: Math.round(progress * 100),
                color: rgbToHex({
                    r: start.r + (end.r - start.r) * progress,
                    g: start.g + (end.g - start.g) * progress,
                    b: start.b + (end.b - start.b) * progress
                })
            };
        });
    }

    // Callers pass a text color that already parsed, so the ratio is never null here.
    private worstContrast(textColor: string, samples: BannerSample[]): BannerSample & { ratio: number } {
        return samples
            .map(sample => ({ ...sample, ratio: safeContrastRatio(textColor, sample.color) ?? 0 }))
            .reduce((lowest, sample) => (sample.ratio < lowest.ratio ? sample : lowest));
    }

    // Walks lightness down and up from the current color and keeps the smallest change that
    // passes on every sample. Black and white are the last steps, so null means nothing passes.
    private suggestTextColor(textColor: string, samples: BannerSample[]): string | null {
        if (!samples.length) return null;
        for (let delta = 2; delta <= 100; delta += 2) {
            for (const candidate of [adjustLightness(textColor, -delta), adjustLightness(textColor, delta)]) {
                if (this.worstContrast(candidate, samples).ratio >= MIN_TEXT_CONTRAST) return candidate;
            }
        }
        return null;
    }
}
