import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { MemberApproval } from './member-approval';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
import { of } from 'rxjs';

describe('MemberApproval Component', () => {
    let component: MemberApproval;
    let fixture: ComponentFixture<MemberApproval>;
    let adminServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getPendingMembers: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 })),
            approveMember: vi.fn().mockReturnValue(of({ success: true })),
            rejectMember: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [MemberApproval],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: LookupService, useValue: { getOptions: vi.fn().mockReturnValue(of([])), getAcademicYears: vi.fn().mockReturnValue(of([])) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(MemberApproval);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load pending members on init', () => {
        expect(adminServiceMock.getPendingMembers).toHaveBeenCalled();
    });
});
