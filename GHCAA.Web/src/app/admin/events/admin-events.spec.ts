import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminEvents } from './admin-events';
import { EventsService } from '../../core/services/events.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { of } from 'rxjs';

describe('AdminEvents Component', () => {
    let component: AdminEvents;
    let fixture: ComponentFixture<AdminEvents>;
    let eventsServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        eventsServiceMock = {
            getAllEventsForAdmin: vi.fn().mockReturnValue(of([])),
            getAllRegistrations: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        const navServiceMock = {
            portalNavItems: () => [],
            adminNavItems: () => [],
            isSuperAdmin: () => false,
            isAdmin: () => false
        };

        await TestBed.configureTestingModule({
            imports: [AdminEvents],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: NavService, useValue: navServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminEvents);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load events and registrations on init', () => {
        expect(eventsServiceMock.getAllEventsForAdmin).toHaveBeenCalled();
        expect(eventsServiceMock.getAllRegistrations).toHaveBeenCalled();
    });

    it('should validate that end date is after start date', () => {
        component.openCreateForm();
        component.eventForm.patchValue({
            startDate: '2023-10-10T10:00',
            endDate: '2023-10-09T10:00'
        });
        
        expect(component.eventForm.errors?.['endBeforeStart']).toBeTruthy();
    });

    it('should validate that registration ends before event starts', () => {
        component.openCreateForm();
        component.eventForm.patchValue({
            startDate: '2023-10-10T10:00',
            registrationEndDate: '2023-10-11T10:00'
        });
        
        expect(component.eventForm.errors?.['regEndAfterEventStart']).toBeTruthy();
    });

    it('should mark all fields as touched and show error on invalid submission', () => {
        const markAllAsTouchedSpy = vi.spyOn(component.eventForm, 'markAllAsTouched');
        component.submitEvent();
        
        expect(markAllAsTouchedSpy).toHaveBeenCalled();
        expect(notificationServiceMock.error).toHaveBeenCalledWith('Please complete all required fields.');
    });

    it('should show specific error message for date validation failure', () => {
        component.openCreateForm();
        component.eventForm.patchValue({
            title: 'Test',
            description: 'Test',
            location: 'Test',
            startDate: '2023-10-10T10:00',
            endDate: '2023-10-09T10:00'
        });
        
        component.submitEvent();
        expect(notificationServiceMock.error).toHaveBeenCalledWith('Event end date must be after the start date.');
    });

    it('should call createEvent on valid submission', () => {
        const eventData = {
            title: 'New Event',
            description: 'Desc',
            location: 'Loc',
            startDate: '2023-12-01T10:00',
            endDate: '2023-12-01T12:00',
            registrationStartDate: '2023-11-01T10:00',
            registrationEndDate: '2023-11-30T10:00',
            registrationFee: 100,
            requiresPayment: true,
            isActive: true,
            allowNonMembers: false
        };
        
        eventsServiceMock.createEvent = vi.fn().mockReturnValue(of({ id: 1 }));
        
        component.openCreateForm();
        component.eventForm.patchValue(eventData);
        
        component.submitEvent();
        
        expect(eventsServiceMock.createEvent).toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Event created!');
    });

    it('should handle logo selection and set preview', async () => {
        const file = new File([''], 'logo.png', { type: 'image/png' });
        const event = { target: { files: [file] } } as any;
        
        // Mock FileReader
        const dummyDataUrl = 'data:image/png;base64,...';
        const readerMock = {
            readAsDataURL: vi.fn(),
            onload: null as any,
            result: null as any
        };
        vi.stubGlobal('FileReader', vi.fn().mockImplementation(function() { return readerMock; }));

        component.onLogoSelected(event);
        
        expect(component.selectedLogo()).toBe(file);
        
        // Simulate reader onload
        if (readerMock.onload) {
            readerMock.result = dummyDataUrl;
            readerMock.onload({ target: { result: dummyDataUrl } } as any);
            expect(component.logoPreview()).toBe(dummyDataUrl);
        }
    });
});

