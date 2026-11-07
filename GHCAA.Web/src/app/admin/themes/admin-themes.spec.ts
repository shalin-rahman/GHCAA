import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminThemes } from './admin-themes';
import { AdminService } from '../../core/services/admin.service';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminThemes Component', () => {
    let component: AdminThemes;
    let fixture: ComponentFixture<AdminThemes>;
    let adminServiceMock: any;
    let themeServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getAllThemes: vi.fn().mockReturnValue(of([])),
            createTheme: vi.fn().mockReturnValue(of({ success: true })),
            updateTheme: vi.fn().mockReturnValue(of({ success: true })),
            deleteTheme: vi.fn().mockReturnValue(of({ success: true }))
        };

        themeServiceMock = {
            loadActiveSpecialTheme: vi.fn()
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminThemes],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: ThemeService, useValue: themeServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminThemes);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load themes on init', () => {
        expect(adminServiceMock.getAllThemes).toHaveBeenCalled();
    });

    it('reports passing and failing contrast for a solid banner', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#000000',
            backgroundColor: '#ffffff',
            sidebarColor: '#111111',
            enableGradientFading: false
        };
        expect(component.contrastAdvice()).toMatchObject({ status: 'pass', ratio: 21 });

        component.selectedTheme.textColor = '#ffffff';
        expect(component.contrastAdvice()).toMatchObject({ status: 'warning', ratio: 1 });
    });

    it('checks the middle of a gradient when its endpoints both pass', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#767676',
            backgroundColor: '#000000',
            sidebarColor: '#ffffff',
            enableGradientFading: true
        };

        // The low point is where the gray matches the text, just under halfway.
        const advice = component.contrastAdvice();
        expect(advice).toMatchObject({ status: 'warning' });
        expect(advice?.status === 'warning' && advice.ratio).toBeLessThan(1.1);
        expect(advice?.status === 'warning' && advice.position).toBeGreaterThanOrEqual(40);
        expect(advice?.status === 'warning' && advice.position).toBeLessThanOrEqual(50);
    });

    it('shows an image preview without text contrast advice', () => {
        component.openCreate();
        component.selectedTheme = {
            ...component.selectedTheme,
            imageUrl: '/assets/special-day.png',
            announcementText: 'Hidden by the banner image'
        };
        fixture.detectChanges();

        expect(component.contrastAdvice()).toBeNull();
        expect(fixture.nativeElement.querySelector('.nav-preview-image')).not.toBeNull();
        expect(fixture.nativeElement.querySelector('.ticker-mock')).toBeNull();
    });

    it('rounds the shown ratio down so a failing value never reads as 4.50', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#777777',
            backgroundColor: '#ffffff',
            enableGradientFading: false
        };
        const advice = component.contrastAdvice();
        expect(advice).toMatchObject({ status: 'warning' });
        const ratio = advice?.status === 'warning' ? advice.ratio : 0;
        const shown = Number(/contrast is ([\d.]+):1/.exec(advice!.message)![1]);
        expect(shown).toBeLessThanOrEqual(ratio);
        expect(shown).toBeLessThan(4.5);
        expect(ratio - shown).toBeLessThan(0.01);
    });

    it('reuses the advice until an input it reads changes', () => {
        component.selectedTheme = { ...component.selectedTheme, textColor: '#000000', backgroundColor: '#ffffff' };
        const first = component.contrastAdvice();
        expect(component.contrastAdvice()).toBe(first);

        component.selectedTheme.title = 'Not read by the check';
        expect(component.contrastAdvice()).toBe(first);

        component.selectedTheme.textColor = '#ffffff';
        expect(component.contrastAdvice()).not.toBe(first);
        expect(component.contrastAdvice()).toMatchObject({ status: 'warning' });
    });

    it('reports invalid hex input without a contrast ratio', () => {
        component.selectedTheme.textColor = '#12';

        expect(component.contrastAdvice()).toEqual({
            status: 'invalid',
            message: 'Enter a hex color like #1a2b3c.'
        });
    });

    it('uses the live banner gradient in the preview', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            backgroundColor: '#123456',
            sidebarColor: '#abcdef',
            enableGradientFading: true
        };

        expect(component.previewBackground()).toBe('linear-gradient(135deg, #123456, #abcdef)');
    });

    it('only applies a suggested text color after an explicit click', () => {
        component.openCreate();
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#000000',
            backgroundColor: '#000000',
            sidebarColor: '#000000',
            enableGradientFading: false
        };
        fixture.detectChanges();

        expect(component.selectedTheme.textColor).toBe('#000000');
        const suggestion = fixture.nativeElement.querySelector('.contrast-check button') as HTMLButtonElement;
        suggestion.click();
        fixture.detectChanges();

        expect(component.selectedTheme.textColor).not.toBe('#000000');
        expect(component.contrastAdvice()).toMatchObject({ status: 'pass' });
        expect(adminServiceMock.createTheme).not.toHaveBeenCalled();
        expect(adminServiceMock.updateTheme).not.toHaveBeenCalled();
    });

    it('finds a gradient low point that falls between quarter marks', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#2a0138',
            backgroundColor: '#e731e9',
            sidebarColor: '#1ca3f0',
            enableGradientFading: true
        };

        const advice = component.contrastAdvice();
        expect(advice).toMatchObject({ status: 'warning' });
        expect(advice?.status === 'warning' && advice.ratio).toBeLessThan(4.45);
        expect(advice?.status === 'warning' && advice.position).toBeGreaterThan(25);
        expect(advice?.status === 'warning' && advice.position).toBeLessThan(50);
    });

    it('ignores sidebarColor when the live banner does not use it', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#ffffff',
            backgroundColor: '#000000',
            sidebarColor: 'not-a-color',
            enableGradientFading: false
        };
        expect(component.contrastAdvice()).toMatchObject({ status: 'pass', ratio: 21 });

        component.selectedTheme.sidebarColor = '';
        component.selectedTheme.enableGradientFading = true;
        expect(component.contrastAdvice()).toMatchObject({ status: 'pass', ratio: 21 });
    });

    it('reports an invalid sidebarColor when the gradient uses it', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            backgroundColor: '#000000',
            sidebarColor: '#12',
            enableGradientFading: true
        };

        expect(component.contrastAdvice()).toMatchObject({ status: 'invalid' });
    });

    it('suggests a text color that passes along the whole gradient', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#999999',
            backgroundColor: '#ffffff',
            sidebarColor: '#ffff00',
            enableGradientFading: true
        };

        component.useSuggestedTextColor();

        expect(component.selectedTheme.textColor).not.toBe('#999999');
        expect(component.contrastAdvice()).toMatchObject({ status: 'pass' });
    });

    it('keeps the text color and warns when no text color can pass', () => {
        component.selectedTheme = {
            ...component.selectedTheme,
            textColor: '#767676',
            backgroundColor: '#000000',
            sidebarColor: '#ffffff',
            enableGradientFading: true
        };

        component.useSuggestedTextColor();

        expect(component.selectedTheme.textColor).toBe('#767676');
        expect(notificationServiceMock.warning).toHaveBeenCalled();
    });
});
