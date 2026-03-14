import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminMembers } from './admin-members';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminMembers Component', () => {
    let component: AdminMembers;
    let fixture: ComponentFixture<AdminMembers>;
    let adminServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getMembersAdmin: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 })),
            exportMembers: vi.fn().mockReturnValue(of(new Blob()))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminMembers],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminMembers);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load members on init', () => {
        expect(adminServiceMock.getMembersAdmin).toHaveBeenCalled();
    });
});
