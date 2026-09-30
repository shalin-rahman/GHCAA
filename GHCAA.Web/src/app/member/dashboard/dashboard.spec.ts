import { createAuthServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dashboard } from './dashboard';
import { ProfileService } from '../../core/services/profile.service';
import { EventsService } from '../../core/services/events.service';
import { NetworkingService } from '../../core/services/networking.service';
import { AlertService } from '../../core/services/alert.service';
import { AuthService } from '../../core/services/auth.service';
import { NewsService } from '../../core/services/news.service';
import { ElectionsService } from '../../core/services/elections.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { MEMBERSHIP_TYPES } from '../../core/constants/app.constants';

describe('Dashboard Component', () => {
    let component: Dashboard;
    let fixture: ComponentFixture<Dashboard>;
    let profileServiceMock: any;
    let eventsServiceMock: any;
    let networkingServiceMock: any;
    let alertServiceMock: any;
    let authServiceMock: any;
    let newsServiceMock: any;
    let electionsServiceMock: any;

    beforeEach(async () => {
        profileServiceMock = {
            getProfile: vi.fn().mockReturnValue(of({ fullName: 'Test' })),
            getMyPendingSummary: vi.fn().mockReturnValue(of(null))
        };
        eventsServiceMock = {
            getEvents: vi.fn().mockReturnValue(of([])),
            getUpcomingEvents: vi.fn().mockReturnValue(of([]))
        };
        networkingServiceMock = {
            getRecentlyJoined: vi.fn().mockReturnValue(of([]))
        };
        alertServiceMock = {
            loadNotifications: vi.fn(),
            unreadCount: vi.fn().mockReturnValue(0),
            notifications: vi.fn().mockReturnValue([])
        };
        authServiceMock = {
            logout: vi.fn()
        };
        newsServiceMock = {
            getNews: vi.fn().mockReturnValue(of([]))
        };
        electionsServiceMock = {
            getMyAppointments: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [Dashboard],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock },
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NetworkingService, useValue: networkingServiceMock },
                { provide: AlertService, useValue: alertServiceMock },
                { provide: AuthService, useValue: authServiceMock },
                { provide: NewsService, useValue: newsServiceMock },
                { provide: ElectionsService, useValue: electionsServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Dashboard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('lists election appointments still waiting for an answer, and not accepted ones', () => {
        electionsServiceMock.getMyAppointments.mockReturnValue(of([
            { id: 1, acceptedAt: null },
            { id: 2, acceptedAt: '2026-09-01T00:00:00Z' }
        ]));
        component.ngOnInit();

        expect(component.pendingRows()).toEqual([
            { label: 'Election appointments awaiting your answer', count: 1, link: '/officials/my-appointments' }
        ]);
    });

    it('should load overview data on init', () => {
        expect(profileServiceMock.getProfile).toHaveBeenCalled();
        expect(eventsServiceMock.getEvents).toHaveBeenCalled();
        expect(networkingServiceMock.getRecentlyJoined).toHaveBeenCalled();
    });

    it('should display contribution points and rank from profile', () => {
        const mockProfile = {
            fullName: 'Test User',
            contributionPoints: 450,
            rank: '12',
            profileCompletionPercentage: 85
        };
        profileServiceMock.getProfile.mockReturnValue(of(mockProfile));
        
        // Re-initialize to pick up mock profile
        component.ngOnInit();
        fixture.detectChanges();
        
        expect(component.contributionPoints).toBe(450);
        expect(component.memberRank).toBe('12');
        expect(component.profileCompletion).toBe(85);
    });

    // 35.2 regression guard — same defect as 35.1, in the dashboard's own copy of the map.
    it('should label the last MembershipType as Guest, never Life', () => {
        expect(component.getMembershipType(6)).toBe('Guest Member');
        expect(component.getMembershipType(6)).not.toContain('Life');
        expect(component.getMembershipType('Guest')).toBe('Guest Member');
    });

    it('should label every MembershipType ordinal from the shared constant', () => {
        expect(MEMBERSHIP_TYPES.map((_, i) => component.getMembershipType(i))).toEqual(MEMBERSHIP_TYPES);
    });
});

