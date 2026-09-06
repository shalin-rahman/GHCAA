import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminGovernance } from './admin-governance';
import { HttpClient } from '@angular/common/http';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminGovernance Component', () => {
    let component: AdminGovernance;
    let fixture: ComponentFixture<AdminGovernance>;
    let httpClientMock: any;
    let adminServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        httpClientMock = {
            get: vi.fn().mockReturnValue(of([])),
            post: vi.fn().mockReturnValue(of({})),
            put: vi.fn().mockReturnValue(of({})),
            delete: vi.fn().mockReturnValue(of({}))
        };
        adminServiceMock = {
            assignECRole: vi.fn().mockReturnValue(of({ success: true })),
            updatePeriod: vi.fn().mockReturnValue(of({ success: true }))
        };
        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminGovernance],
            providers: [
                { provide: HttpClient, useValue: httpClientMock },
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminGovernance);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load periods and committee on init', () => {
        expect(httpClientMock.get).toHaveBeenCalled(); // loadPeriods
    });

    // 82.52: notifyMember defaults to off and is threaded through to the API on both
    // assignment and removal, so an admin's choice per action actually reaches the backend.
    it('assignRole posts notifyMember:false by default', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 8, reason: '', notifyMember: false });

        component.assignRole();

        expect(httpClientMock.post).toHaveBeenCalledWith(
            expect.any(String),
            expect.objectContaining({ notifyMember: false })
        );
    });

    it('assignRole posts notifyMember:true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 8, reason: '', notifyMember: true });

        component.assignRole();

        expect(httpClientMock.post).toHaveBeenCalledWith(
            expect.any(String),
            expect.objectContaining({ notifyMember: true })
        );
    });

    it('confirmRemoveMember deletes with notifyMember=false by default', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);

        component.confirmRemoveMember();

        expect(httpClientMock.delete).toHaveBeenCalledWith(expect.stringContaining('notifyMember=false'));
    });

    it('confirmRemoveMember deletes with notifyMember=true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);
        component.removeNotifyMember.set(true);

        component.confirmRemoveMember();

        expect(httpClientMock.delete).toHaveBeenCalledWith(expect.stringContaining('notifyMember=true'));
    });
});
