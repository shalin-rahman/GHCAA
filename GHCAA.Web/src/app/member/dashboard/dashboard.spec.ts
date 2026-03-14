import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dashboard } from './dashboard';
import { ProfileService } from '../../core/services/profile.service';
import { NewsService } from '../../core/services/news.service';
import { EventsService } from '../../core/services/events.service';
import { of } from 'rxjs';

describe('Dashboard Component', () => {
    let component: Dashboard;
    let fixture: ComponentFixture<Dashboard>;
    let profileServiceMock: any;
    let newsServiceMock: any;
    let eventsServiceMock: any;

    beforeEach(async () => {
        profileServiceMock = {
            getProfile: vi.fn().mockReturnValue(of({ fullName: 'Test' }))
        };
        newsServiceMock = {
            getLatestNews: vi.fn().mockReturnValue(of([]))
        };
        eventsServiceMock = {
            getUpcomingEvents: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [Dashboard],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock },
                { provide: NewsService, useValue: newsServiceMock },
                { provide: EventsService, useValue: eventsServiceMock }
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
        expect(newsServiceMock.getLatestNews).toHaveBeenCalled();
        expect(eventsServiceMock.getUpcomingEvents).toHaveBeenCalled();
    });
});
