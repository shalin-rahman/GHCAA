import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EventsService } from './events.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('EventsService', () => {
    let service: EventsService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [EventsService]
        });
        service = TestBed.inject(EventsService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should fetch events', () => {
        const dummyEvents = [{ id: 1, title: 'Event 1' }];
        service.getEvents().subscribe(events => {
            expect(events.length).toBe(1);
            expect(events).toEqual(dummyEvents as any);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.EVENTS);
        expect(req.request.method).toBe('GET');
        req.flush(dummyEvents);
    });
});
