import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminMembers } from './admin-members';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NavService } from '../../core/services/nav.service';
import { LookupService } from '../../core/services/lookup.service';
import { EcPositionService } from '../../core/services/ec-position.service';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';

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

        notificationServiceMock = createNotificationServiceMock();

        const confirmDialogServiceMock = {
            confirm: vi.fn().mockReturnValue(of(true))
        };

        navServiceMock = {
            isSuperAdmin: signal(false) as any
        };

        routerMock = {
            navigate: vi.fn()
        };
        activatedRouteMock = {
            snapshot: { queryParamMap: convertToParamMap({}) }
        };

        const positionServiceMock = {
            getActive: vi.fn().mockReturnValue(of([
                { id: 1, key: 'President', name: 'President', seatCount: 1 },
                { id: 16, key: 'Patron', name: 'Patron', seatCount: 2 }
            ]))
        };
        const lookupServiceMock = {
            getOptions: vi.fn().mockReturnValue(of([])),
            getAcademicYears: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [AdminMembers],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ConfirmDialogService, useValue: confirmDialogServiceMock },
                { provide: NavService, useValue: navServiceMock },
                { provide: Router, useValue: routerMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock },
                { provide: LookupService, useValue: lookupServiceMock },
                { provide: EcPositionService, useValue: positionServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminMembers);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('FR-34: picking a position on a history row sets its id, key and name together', () => {
        const row: any = { positionId: 0, position: '', positionName: '' };

        component.onHistoryPositionChange(row, 16);

        expect(row).toEqual({ positionId: 16, position: 'Patron', positionName: 'Patron' });
    });

    it('FR-34: the current position label uses the name the API sends', () => {
        const member = { ecHistory: [{ position: 'Patron', positionName: 'Patron', isCurrent: true }] };

        expect(component.getCurrentPosition(member)).toBe('Patron');
        expect(component.getCurrentPosition({ ecHistory: [] })).toBe('None');
    });

    it('FR-34: a term on a retired position keeps its label as a disabled option', () => {
        expect(component.isRetiredPosition(3)).toBe(true);
        expect(component.isRetiredPosition(16)).toBe(false);
        expect(component.isRetiredPosition(0)).toBe(false);
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
        component.sendResetLink(member.id);
        expect(adminServiceMock.sendPasswordResetLink).toHaveBeenCalledWith(member.id);
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Success');
    });

    it('should re-enable Save when the document upload fails after the data saved', () => {
        adminServiceMock.updateMember = vi.fn().mockReturnValue(of({}));
        adminServiceMock.updateMemberDocuments = vi.fn().mockReturnValue(throwError(() => new Error('upload')));
        component.selectedMember.set({ id: 7, fullName: 'Test User', mobileNo: '01712345678', email: 't@example.com' });
        component.certToUpload = new File(['x'], 'cert.pdf');

        component.saveMember();

        expect(notificationServiceMock.error).toHaveBeenCalledWith('Data updated, but document upload failed.');
        expect(component.submitting()).toBe(false);
    });

    // The public directory's admin Edit button lands here with ?edit=<id>.
    it('opens the member in edit mode when the edit query param is set', () => {
        adminServiceMock.getMemberById = vi.fn().mockReturnValue(of({ id: 7, fullName: 'Test User' }));
        adminServiceMock.getMemberPayments = vi.fn().mockReturnValue(of([]));
        activatedRouteMock.snapshot.queryParamMap = convertToParamMap({ edit: '7' });

        component.ngOnInit();

        expect(adminServiceMock.getMemberById).toHaveBeenCalledWith(7);
        expect(component.selectedMember()?.id).toBe(7);
        expect(component.isEditing()).toBe(true);
    });

    it('ignores an edit query param that is not a member id', () => {
        adminServiceMock.getMemberById = vi.fn();
        activatedRouteMock.snapshot.queryParamMap = convertToParamMap({ edit: 'abc' });

        component.ngOnInit();

        expect(adminServiceMock.getMemberById).not.toHaveBeenCalled();
    });

    it('opens a member in view mode from the list', () => {
        adminServiceMock.getMemberById = vi.fn().mockReturnValue(of({ id: 8, fullName: 'Test User' }));
        adminServiceMock.getMemberPayments = vi.fn().mockReturnValue(of([]));

        component.openDetail({ id: 8 });

        expect(component.isEditing()).toBe(false);
    });
});
