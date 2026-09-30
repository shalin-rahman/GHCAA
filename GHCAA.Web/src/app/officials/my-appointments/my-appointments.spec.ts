// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { MyAppointments } from './my-appointments';
import { ElectionsService } from '../../core/services/elections.service';
import { NotificationService } from '../../core/services/notification.service';

describe('MyAppointments', () => {
    let elections: any;
    let notify: any;

    const pending = { id: 7, personaName: 'Polling Officer', electionTitle: 'EC 2026', acceptedAt: null, isLive: false };

    function create(): MyAppointments {
        elections = {
            getMyAppointments: vi.fn().mockReturnValue(of([pending])),
            acceptAppointment: vi.fn().mockReturnValue(of(undefined)),
            declineAppointment: vi.fn().mockReturnValue(of(undefined))
        };
        notify = { success: vi.fn(), error: vi.fn() };
        TestBed.configureTestingModule({
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: NotificationService, useValue: notify }
            ]
        });
        return TestBed.runInInjectionContext(() => new MyAppointments());
    }

    it('will not accept until the declaration is ticked', () => {
        const page = create();

        page.accept(pending as any);

        expect(elections.acceptAppointment).not.toHaveBeenCalled();
        expect(notify.error).toHaveBeenCalledWith('Tick the declaration first.');
    });

    it('accepts once ticked and reloads the list', () => {
        const page = create();
        page.agreed[7] = true;

        page.accept(pending as any);

        expect(elections.acceptAppointment).toHaveBeenCalledWith(7);
        expect(elections.getMyAppointments).toHaveBeenCalledTimes(2);
    });

    it('drops a declined appointment and sends the trimmed reason', () => {
        const page = create();
        page.reasons[7] = '  away that week  ';

        page.decline(pending as any);

        expect(elections.declineAppointment).toHaveBeenCalledWith(7, 'away that week');
        expect(page.appointments()).toEqual([]);
    });

    it('shows the server reason when accept fails', () => {
        const page = create();
        page.agreed[7] = true;
        elections.acceptAppointment.mockReturnValue(throwError(() => ({ error: { detail: 'Appointment has expired.' } })));

        page.accept(pending as any);

        expect(notify.error).toHaveBeenCalledWith('Appointment has expired.');
        expect(page.busyId()).toBeNull();
    });
});
