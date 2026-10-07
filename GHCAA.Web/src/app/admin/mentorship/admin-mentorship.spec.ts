import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminMentorship } from './admin-mentorship';
import { MentorshipService } from '../../core/services/mentorship.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { MentorshipAdminRow } from '../../core/models/business.models';
import { of, throwError } from 'rxjs';

describe('AdminMentorship Component', () => {
    let component: AdminMentorship;
    let fixture: ComponentFixture<AdminMentorship>;
    let mentorshipServiceMock: any;
    let notifyMock: any;
    let confirmMock: any;

    const rows: MentorshipAdminRow[] = [
        { id: 1, domain: 'Software Engineering', status: 'Pending', requestedAt: '2026-01-01T00:00:00Z', requester: { id: 1, fullName: 'Alice' }, mentor: { id: 2, fullName: 'Bob' } },
        { id: 2, domain: 'Finance', status: 'Accepted', requestedAt: '2026-01-02T00:00:00Z', requester: { id: 3, fullName: 'Carol' }, mentor: { id: 4, fullName: 'Dave' } },
        { id: 3, domain: 'Law', status: 'Completed', requestedAt: '2026-01-03T00:00:00Z', requester: { id: 5, fullName: 'Erin' }, mentor: { id: 6, fullName: 'Frank' } }
    ];

    beforeEach(async () => {
        mentorshipServiceMock = {
            getAllForAdmin: vi.fn().mockReturnValue(of(rows)),
            adminClose: vi.fn().mockReturnValue(of(undefined))
        };
        notifyMock = { success: vi.fn(), error: vi.fn() };
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };

        await TestBed.configureTestingModule({
            imports: [AdminMentorship],
            providers: [
                { provide: MentorshipService, useValue: mentorshipServiceMock },
                { provide: NotificationService, useValue: notifyMock },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminMentorship);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load requests on init', () => {
        expect(mentorshipServiceMock.getAllForAdmin).toHaveBeenCalled();
        expect(component.requests().length).toBe(3);
    });

    it('should filter requests by requester, mentor, or domain', () => {
        component.searchTerm.set('finance');
        expect(component.filteredRequests().length).toBe(1);
        expect(component.filteredRequests()[0].id).toBe(2);
    });

    it('should count requests per status tab', () => {
        expect(component.counts()).toEqual({ All: 3, Pending: 1, Accepted: 1, Completed: 1, Declined: 0 });
    });

    it('should count a numeric status the same as its name', () => {
        mentorshipServiceMock.getAllForAdmin.mockReturnValue(of([{ ...rows[0], status: 0 as any }]));
        (component as any).loadRequests();
        expect(component.counts().Pending).toBe(1);
    });

    it('should filter by tab and combine with the search term', () => {
        component.activeTab.set('Accepted');
        expect(component.filteredRequests().map(r => r.id)).toEqual([2]);
        component.searchTerm.set('alice');
        expect(component.filteredRequests().length).toBe(0);
    });

    it('should only allow closing open requests, with the matching outcome', () => {
        expect(component.canClose(rows[0])).toBe(true);
        expect(component.closeOutcome(rows[0])).toBe('Declined');
        expect(component.canClose(rows[1])).toBe(true);
        expect(component.closeOutcome(rows[1])).toBe('Completed');
        expect(component.canClose(rows[2])).toBe(false);
    });

    it('should reset the note when a request is opened', () => {
        component.closeNote.set('old');
        component.open(rows[0]);
        expect(component.selected()).toBe(rows[0]);
        expect(component.closeNote()).toBe('');
    });

    it('should close a request with the trimmed note and reload', async () => {
        component.open(rows[1]);
        component.closeNote.set('  Programme ended  ');
        mentorshipServiceMock.getAllForAdmin.mockClear();

        await component.closeRequest(rows[1]);

        expect(mentorshipServiceMock.adminClose).toHaveBeenCalledWith(2, 'Programme ended');
        expect(notifyMock.success).toHaveBeenCalledWith('Request marked as Completed.');
        expect(component.selected()).toBeNull();
        expect(component.closing()).toBe(false);
        expect(mentorshipServiceMock.getAllForAdmin).toHaveBeenCalled();
    });

    it('should send no note when the note is blank', async () => {
        component.closeNote.set('   ');
        await component.closeRequest(rows[0]);
        expect(mentorshipServiceMock.adminClose).toHaveBeenCalledWith(1, undefined);
    });

    it('should do nothing when the admin cancels the confirm', async () => {
        confirmMock.confirm.mockReturnValue(of(false));
        await component.closeRequest(rows[0]);
        expect(mentorshipServiceMock.adminClose).not.toHaveBeenCalled();
    });

    it('should not offer close on a finished request', async () => {
        await component.closeRequest(rows[2]);
        expect(confirmMock.confirm).not.toHaveBeenCalled();
        expect(mentorshipServiceMock.adminClose).not.toHaveBeenCalled();
    });

    it('should keep the dialog open and show an error when close fails', async () => {
        mentorshipServiceMock.adminClose.mockReturnValue(throwError(() => new Error('409')));
        component.open(rows[0]);

        await component.closeRequest(rows[0]);

        expect(notifyMock.error).toHaveBeenCalled();
        expect(component.selected()).toBe(rows[0]);
        expect(component.closing()).toBe(false);
    });

    it('should block dismiss while a close is in flight', () => {
        component.open(rows[0]);
        component.closing.set(true);
        component.dismiss();
        expect(component.selected()).toBe(rows[0]);
    });

    it('should show an error toast when loading fails', () => {
        mentorshipServiceMock.getAllForAdmin.mockReturnValue(throwError(() => new Error('500')));
        (component as any).loadRequests();
        expect(notifyMock.error).toHaveBeenCalledWith('Could not load mentorship requests.');
        expect(component.loading()).toBe(false);
    });
});
