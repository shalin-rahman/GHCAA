import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Directory } from './directory';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';

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
                { provide: Router, useValue: routerMock }
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
        expect(component.membershipTypes).toBe(MEMBERSHIP_TYPE_OPTIONS);
        expect(component.membershipTypes.map(t => t.value)).toContain('Guest');
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


