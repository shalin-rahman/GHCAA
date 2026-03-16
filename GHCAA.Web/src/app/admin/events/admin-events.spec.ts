import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminEvents } from './admin-events';
import { EventsService } from '../../core/services/events.service';
import { NotificationService } from '../../core/services/notification.service';
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

        await TestBed.configureTestingModule({
            imports: [AdminEvents],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
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
