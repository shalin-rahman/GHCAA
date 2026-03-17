import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminMembers } from './admin-members';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

import { ActivatedRoute, Router } from '@angular/router';

describe('AdminMembers Component', () => {
    let component: AdminMembers;
    let fixture: ComponentFixture<AdminMembers>;
    let adminServiceMock: any;
    let notificationServiceMock: any;
    let routerMock: any;
    let activatedRouteMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getMembers: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 })),
            getPeriods: vi.fn().mockReturnValue(of([])),
            exportMembers: vi.fn().mockReturnValue(of(new Blob())),
            sendPasswordResetLink: vi.fn().mockReturnValue(of({ message: 'Success' }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        routerMock = {
            navigate: vi.fn()
        };
        activatedRouteMock = {
            snapshot: {}
        };

        await TestBed.configureTestingModule({
            imports: [AdminMembers],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: Router, useValue: routerMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock }
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
        expect(adminServiceMock.getMembers).toHaveBeenCalled();
    });
    
    it('should call sendPasswordResetLink and notify success', () => {
        // Mock a selected member
        const member = { id: 100, fullName: 'Test User' };
        component.selectedMember.set(member);
        
        // Mock confirmation
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        
        component.sendResetLink(member.id);
        
        expect(adminServiceMock.sendPasswordResetLink).toHaveBeenCalledWith(member.id);
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Password reset link sent.');
    });
});
