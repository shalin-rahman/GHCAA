import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Directory } from './directory';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { LOOKUP_GROUPS, MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';
import { LookupService } from '../../core/services/lookup.service';

describe('Directory Component', () => {
    let component: Directory;
    let fixture: ComponentFixture<Directory>;
    let networkServiceMock: any;
    let notificationServiceMock: any;
    let routerMock: any;

    beforeEach(async () => {
        vi.useFakeTimers();

        networkServiceMock = {
            searchMembers: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 20, hasNextPage: false }))
        };
        notificationServiceMock = createNotificationServiceMock();
        routerMock = {
            navigate: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [Directory],
            providers: [
                { provide: NetworkingService, useValue: networkServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: Router, useValue: routerMock },
                {
                    provide: LookupService, useValue: {
                        // 62.33: MembershipType now goes through getOptions() like MemberCategory —
                        // mirror the call-first-then-fallback shape by answering with the same
                        // fallback list a real empty lookups table would return.
                        getOptions: vi.fn().mockImplementation((group: string) =>
                            of(group === LOOKUP_GROUPS.MembershipType ? MEMBERSHIP_TYPE_OPTIONS : [])),
                        getAcademicYears: vi.fn().mockReturnValue(of([]))
                    }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Directory);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        vi.restoreAllMocks();
        vi.useRealTimers();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should search on init', () => {
        vi.advanceTimersByTime(300);
        expect(networkServiceMock.searchMembers).toHaveBeenCalled();
    });

    it('should update filters and trigger search', () => {
        component.filters.query = 'John';
        component.search();
        vi.advanceTimersByTime(300);
        
        expect(networkServiceMock.searchMembers).toHaveBeenCalledWith(expect.objectContaining({
            query: 'John'
        }));
    });

    it('should load next page when loadNextPage is called', async () => {
        component.hasMore.set(true);
        networkServiceMock.searchMembers.mockReturnValue(of({ items: [{ id: 2 }], page: 2, totalItems: 2, hasNextPage: false }));
        
        await component.loadNextPage();
        
        expect(networkServiceMock.searchMembers).toHaveBeenCalledWith(expect.objectContaining({ page: 2 }));
        expect(component.members().length).toBe(1);
        expect(component.hasMore()).toBe(false);
    });

    // NFR-P1: the footer is the shared load-more, wired to the next directory page.
    it('asks for the next page from the shared load-more footer', async () => {
        networkServiceMock.searchMembers.mockReturnValue(of({ items: [{ id: 1, fullName: 'Ann One', ecHistory: [] }], page: 1, totalItems: 2, hasNextPage: true }));
        component.search();
        vi.advanceTimersByTime(300);
        await Promise.resolve();
        fixture.detectChanges();

        networkServiceMock.searchMembers.mockReturnValue(of({ items: [{ id: 2, fullName: 'Ben Two', ecHistory: [] }], page: 2, totalItems: 2, hasNextPage: false }));
        fixture.nativeElement.querySelector('app-load-more button').click();

        expect(networkServiceMock.searchMembers).toHaveBeenLastCalledWith(expect.objectContaining({ page: 2 }));
        expect(component.members().map((m: any) => m.id)).toEqual([1, 2]);
    });

    // NFR-P1: a page that belongs to the previous filters must not join the new results.
    it('drops a next page that lands after the filters change', async () => {
        component.hasMore.set(true);
        const oldPage = new Subject<any>();
        networkServiceMock.searchMembers.mockReturnValue(oldPage);
        component.loadNextPage();

        networkServiceMock.searchMembers.mockReturnValue(of({ items: [{ id: 9, fullName: 'New Match', ecHistory: [] }], page: 1, totalItems: 1, hasNextPage: false }));
        component.filters.query = 'New';
        component.search();
        vi.advanceTimersByTime(300);
        await Promise.resolve();

        oldPage.next({ items: [{ id: 2, fullName: 'Old Match', ecHistory: [] }], page: 2, totalItems: 30, hasNextPage: true });
        await Promise.resolve();

        expect(component.members().map((m: any) => m.id)).toEqual([9]);
        expect(component.hasMore()).toBe(false);
        expect(component.loadingMore()).toBe(false);
        expect(component.loading()).toBe(false);
    });

    it('should call getMemberProfile when viewProfile is called', () => {
        const mockProfile = { id: 5, fullName: 'Test User' };
        networkServiceMock.getMemberProfile = vi.fn().mockReturnValue(of(mockProfile));
        
        component.viewProfile(5);
        
        expect(networkServiceMock.getMemberProfile).toHaveBeenCalledWith(5);
        expect(component.selectedMember()).toEqual(mockProfile);
    });

    it('should navigate to messages and clear selected member when sendMessage is called', () => {
        component.selectedMember.set({ id: 5 });
        component.sendMessage(5);
        
        expect(routerMock.navigate).toHaveBeenCalledWith(['/portal/messages'], { queryParams: { thread: 5 } });
        expect(component.selectedMember()).toBeNull();
    });

    // 35.3 regression guard: the Membership Type filter was a hardcoded <option> list that
    // stopped at Advisory, so Guest members could never be selected — they were hidden.
    it('should offer every MembershipType in the filter, including Guest', () => {
        expect(component.membershipTypes()).toEqual(MEMBERSHIP_TYPE_OPTIONS);
        expect(component.membershipTypes().map(t => t.value)).toContain('Guest');
    });

    it('should pass a Guest membership-type filter through to the search', () => {
        component.filters.membershipType = 'Guest';
        component.search();
        vi.advanceTimersByTime(300);

        expect(networkServiceMock.searchMembers).toHaveBeenCalledWith(expect.objectContaining({
            membershipType: 'Guest'
        }));
    });
});


