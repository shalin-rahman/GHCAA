import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Events } from './events';
import { EventsService } from '../../core/services/events.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

import { ActivatedRoute, provideRouter } from '@angular/router';

describe('Events Component', () => {
    let component: Events;
    let fixture: ComponentFixture<Events>;
    let eventsServiceMock: any;
    let notificationServiceMock: any;
    let activatedRouteMock: any;

    beforeEach(async () => {
        eventsServiceMock = {
            getEvents: vi.fn().mockReturnValue(of([])),
            getUpcomingEvents: vi.fn().mockReturnValue(of([])),
            getMyRegistrations: vi.fn().mockReturnValue(of([])),
            registerForEvent: vi.fn().mockReturnValue(of({ success: true })),
            getRegistrationForInvitation: vi.fn().mockReturnValue(of({}))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        activatedRouteMock = {
            snapshot: { queryParamMap: { get: vi.fn().mockReturnValue(null) } }
        };

        const authServiceMock = {
            isAuthenticated: vi.fn().mockReturnValue(false),
            currentUser: vi.fn().mockReturnValue(null),
            getToken: vi.fn().mockReturnValue(null)
        };

        await TestBed.configureTestingModule({
            imports: [Events],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock },
                { provide: AuthService, useValue: authServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Events);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load events on init', () => {
        expect(eventsServiceMock.getEvents).toHaveBeenCalled();
    });
});
