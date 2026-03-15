import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Events } from './events';
import { EventsService } from '../../core/services/events.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

import { ActivatedRoute } from '@angular/router';

describe('Events Component', () => {
    let component: Events;
    let fixture: ComponentFixture<Events>;
    let eventsServiceMock: any;
    let notificationServiceMock: any;
    let activatedRouteMock: any;

    beforeEach(async () => {
        eventsServiceMock = {
            getEvents: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 })),
            getUpcomingEvents: vi.fn().mockReturnValue(of([])),
            registerForEvent: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        activatedRouteMock = {
            snapshot: { queryParamMap: { get: vi.fn().mockReturnValue(null) } }
        };

        await TestBed.configureTestingModule({
            imports: [Events],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock }
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
