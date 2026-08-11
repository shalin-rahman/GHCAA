import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminMembers } from './admin-members';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { of } from 'rxjs';
import { signal } from '@angular/core';

import { ActivatedRoute, Router } from '@angular/router';

describe('AdminMembers Component', () => {
    let component: AdminMembers;
    let fixture: ComponentFixture<AdminMembers>;
    let adminServiceMock: any;
    let notificationServiceMock: any;
    let navServiceMock: any;
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

        navServiceMock = {
            isSuperAdmin: signal(false) as any
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
                { provide: NavService, useValue: navServiceMock },
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

    it('should hide import controls for standard Admin', () => {
        navServiceMock.isSuperAdmin.set(false);
        fixture.detectChanges();
        const compiled = fixture.nativeElement;
        expect(compiled.querySelector('.import-btn')).toBeNull();
    });

    it('should call sendPasswordResetLink and notify success', () => {
        const member = { id: 100, fullName: 'Test User' };
        component.selectedMember.set(member);
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        component.sendResetLink(member.id);
        expect(adminServiceMock.sendPasswordResetLink).toHaveBeenCalledWith(member.id);
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Success');
    });
});
