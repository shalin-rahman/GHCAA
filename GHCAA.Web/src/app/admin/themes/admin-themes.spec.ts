import { ComponentFixture, TestBed } from '@angular/core/testing';
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

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

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
});
