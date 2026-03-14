import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
    let service: NotificationService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [NotificationService]
        });
        service = TestBed.inject(NotificationService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should add a toast', () => {
        service.success('Success message');
        expect(service.toasts().length).toBe(1);
        expect(service.toasts()[0].message).toBe('Success message');
        expect(service.toasts()[0].type).toBe('success');
    });

    it('should remove a toast after timeout', fakeAsync(() => {
        service.info('Info message');
        expect(service.toasts().length).toBe(1);
        tick(5000);
        expect(service.toasts().length).toBe(0);
    }));

    it('should remove toast by id', () => {
        service.warning('Warning');
        const id = service.toasts()[0].id;
        service.remove(id);
        expect(service.toasts().length).toBe(0);
    });
});
