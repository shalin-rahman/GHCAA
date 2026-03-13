import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EventsService, AlumniEvent, EventRegistration } from './events.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('EventsService', () => {
    let service: EventsService;
    let httpMock: HttpTestingController;

    const mockEvent: AlumniEvent = {
        id: 1,
        title: 'Reunion',
        description: 'Test Reunion',
        date: '2026-03-08T00:00:00Z',
        location: 'Dhaka',
        isActive: true,
        allowNonMembers: false
    };

    const mockRegistration: EventRegistration = {
        id: 1,
        eventId: 1,
        memberId: 10,
        paymentReference: 'TRX123',
        status: 'Pending',
        registeredAt: '2026-03-08T00:00:00Z',
        isNonMember: false
    };

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

    it('should get active events', () => {
        service.getEvents().subscribe(res => {
            expect(res.length).toBe(1);
            expect(res[0].id).toBe(1);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.EVENTS);
        expect(req.request.method).toBe('GET');
        req.flush([mockEvent]);
    });

    it('should get event by id', () => {
        service.getEventById(1).subscribe(res => {
            expect(res.title).toBe('Reunion');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/1`);
        expect(req.request.method).toBe('GET');
        req.flush(mockEvent);
    });

    it('should register for event', () => {
        const file = new File([''], 'receipt.png');
        service.registerForEvent({ eventId: 1, paymentReference: 'TRX', receiptFile: file }).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/register`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body instanceof FormData).toBe(true);
        req.flush({ success: true });
    });

    it('should get my registrations', () => {
        service.getMyRegistrations().subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/my-registrations`);
        expect(req.request.method).toBe('GET');
        req.flush([mockRegistration]);
    });

    it('should get all events for admin', () => {
        service.getAllEventsForAdmin().subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin/all`);
        expect(req.request.method).toBe('GET');
        req.flush([mockEvent]);
    });

    it('should create event', () => {
        service.createEvent({ title: 'New Event' }).subscribe(res => {
            expect(res.id).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ title: 'New Event' });
        req.flush(mockEvent);
    });

    it('should update event', () => {
        service.updateEvent(1, { title: 'Updated' }).subscribe(res => {
            expect(res.id).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin/1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual({ title: 'Updated' });
        req.flush(mockEvent);
    });

    it('should delete event', () => {
        service.deleteEvent(1).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin/1`);
        expect(req.request.method).toBe('DELETE');
        req.flush({ success: true });
    });

    it('should approve registration', () => {
        service.approveRegistration(1, true).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin/approve-registration`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ registrationId: 1, approve: true });
        req.flush({ success: true });
    });

    it('should get all registrations for admin', () => {
        service.getAllRegistrations().subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.EVENTS}/admin/registrations?page=1&pageSize=10`);
        expect(req.request.method).toBe('GET');
        req.flush([mockRegistration]);
    });
});
