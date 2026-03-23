import { createAuthServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dashboard } from './dashboard';
import { ProfileService } from '../../core/services/profile.service';
import { JobService } from '../../core/services/job.service';
import { EventsService } from '../../core/services/events.service';
import { NetworkingService } from '../../core/services/networking.service';
import { AlertService } from '../../core/services/alert.service';
import { AuthService } from '../../core/services/auth.service';
import { NewsService } from '../../core/services/news.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';

describe('Dashboard Component', () => {
    let component: Dashboard;
    let fixture: ComponentFixture<Dashboard>;
    let profileServiceMock: any;
    let jobServiceMock: any;
    let eventsServiceMock: any;
    let networkingServiceMock: any;
    let alertServiceMock: any;
    let authServiceMock: any;
    let newsServiceMock: any;

    beforeEach(async () => {
        profileServiceMock = {
            getProfile: vi.fn().mockReturnValue(of({ fullName: 'Test' }))
        };
        jobServiceMock = {
            getJobs: vi.fn().mockReturnValue(of([]))
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

        await TestBed.configureTestingModule({
            imports: [Dashboard],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock },
                { provide: JobService, useValue: jobServiceMock },
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NetworkingService, useValue: networkingServiceMock },
                { provide: AlertService, useValue: alertServiceMock },
                { provide: AuthService, useValue: authServiceMock },
                { provide: NewsService, useValue: newsServiceMock },
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

    it('should load overview data on init', () => {
        expect(profileServiceMock.getProfile).toHaveBeenCalled();
        expect(jobServiceMock.getJobs).toHaveBeenCalled();
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
});

