import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminMentorship } from './admin-mentorship';
import { MentorshipService } from '../../core/services/mentorship.service';
import { MentorshipAdminRow } from '../../core/models/business.models';
import { of } from 'rxjs';

describe('AdminMentorship Component', () => {
    let component: AdminMentorship;
    let fixture: ComponentFixture<AdminMentorship>;
    let mentorshipServiceMock: any;

    const rows: MentorshipAdminRow[] = [
        { id: 1, domain: 'Software Engineering', status: 'Pending', requestedAt: '2026-01-01T00:00:00Z', requester: { id: 1, fullName: 'Alice' }, mentor: { id: 2, fullName: 'Bob' } },
        { id: 2, domain: 'Finance', status: 'Accepted', requestedAt: '2026-01-02T00:00:00Z', requester: { id: 3, fullName: 'Carol' }, mentor: { id: 4, fullName: 'Dave' } }
    ];

    beforeEach(async () => {
        mentorshipServiceMock = {
            getAllForAdmin: vi.fn().mockReturnValue(of(rows))
        };

        await TestBed.configureTestingModule({
            imports: [AdminMentorship],
            providers: [
                { provide: MentorshipService, useValue: mentorshipServiceMock }
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
        expect(component.requests().length).toBe(2);
    });

    it('should filter requests by requester, mentor, or domain', () => {
        component.searchTerm.set('finance');
        expect(component.filteredRequests().length).toBe(1);
        expect(component.filteredRequests()[0].id).toBe(2);
    });
});
