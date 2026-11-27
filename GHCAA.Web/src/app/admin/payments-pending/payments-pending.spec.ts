import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { PaymentsPending, visibleLength } from './payments-pending';
import { FinancialService, PendingPayment } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';

describe('PaymentsPending', () => {
    let fixture: ComponentFixture<PaymentsPending>;
    let component: PaymentsPending;
    let financial: any;
    let notify: any;
    let confirm: any;

    const payment: PendingPayment = {
        id: 7, memberId: 12, memberName: 'Rahim Uddin', dueId: null, amount: 1000,
        transactionReference: 'TX-77', proofPath: 'secure_uploads/12/proof.jpg', submittedAt: '2026-11-20T10:00:00Z',
    };

    beforeEach(async () => {
        financial = {
            getPendingPayments: vi.fn().mockReturnValue(of([payment])),
            verifyPayment: vi.fn().mockReturnValue(of({ dueId: 3 })),
            rejectPayment: vi.fn().mockReturnValue(of(undefined)),
            getProof: vi.fn().mockReturnValue(of(new Blob(['x']))),
        };
        notify = createNotificationServiceMock();
        confirm = { confirm: vi.fn().mockReturnValue(of(true)) };

        await TestBed.configureTestingModule({
            imports: [PaymentsPending],
            providers: [
                { provide: FinancialService, useValue: financial },
                { provide: NotificationService, useValue: notify },
                { provide: ConfirmDialogService, useValue: confirm },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(PaymentsPending);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('lists pending payments', () => {
        const rows = fixture.nativeElement.querySelectorAll('tbody tr');
        expect(rows.length).toBe(1);
        expect(rows[0].textContent).toContain('Rahim Uddin');
        expect(rows[0].textContent).toContain('TX-77');
        expect(rows[0].querySelector('[data-testid="archived-tag"]')).toBeNull();
    });

    it('tags an archived member by name', () => {
        financial.getPendingPayments.mockReturnValue(of([{ ...payment, memberArchived: true }]));
        component.load();
        fixture.detectChanges();
        const row = fixture.nativeElement.querySelector('tbody tr');
        expect(row.textContent).toContain('Rahim Uddin');
        expect(row.querySelector('[data-testid="archived-tag"]').textContent).toContain('Archived');
    });

    it('shows the empty state when nothing is pending', () => {
        financial.getPendingPayments.mockReturnValue(of([]));
        component.load();
        fixture.detectChanges();
        expect(fixture.nativeElement.textContent).toContain('No payments are waiting for review.');
    });

    it('reject stays disabled under 3 characters', () => {
        component.openReject(payment);
        component.rejectReason.set(' a​b  ');
        fixture.detectChanges();
        const btn: HTMLButtonElement = fixture.nativeElement.querySelector('[data-testid="confirm-reject"]');
        expect(btn.disabled).toBe(true);

        component.rejectReason.set('abc');
        fixture.detectChanges();
        expect(btn.disabled).toBe(false);
    });

    it('leaves the reference out of the reject summary when there is none', () => {
        const summary = () => fixture.nativeElement.querySelector('[data-testid="reject-summary"]').textContent as string;
        component.openReject(payment);
        fixture.detectChanges();
        expect(summary()).toContain('reference TX-77');

        component.openReject({ ...payment, transactionReference: null });
        fixture.detectChanges();
        expect(summary()).not.toContain('reference');
    });

    it('counts only visible characters', () => {
        expect(visibleLength('  a \n b\t')).toBe(2);
        expect(visibleLength('')).toBe(0);
        expect(visibleLength('abc')).toBe(3);
    });

    it('reject sends the trimmed reason and reloads', () => {
        component.openReject(payment);
        component.rejectReason.set('  Wrong reference  ');
        component.reject();
        expect(financial.rejectPayment).toHaveBeenCalledWith(7, 'Wrong reference');
        expect(component.rejecting()).toBeNull();
        expect(financial.getPendingPayments).toHaveBeenCalledTimes(2);
    });

    it('a 400 on reject keeps the dialog open with the server message', () => {
        financial.rejectPayment.mockReturnValue(throwError(() => new HttpErrorResponse({
            status: 400, error: { title: 'One or more validation errors occurred.', errors: { Reason: ['Give a reason of at least 3 characters.'] } },
        })));
        component.openReject(payment);
        component.rejectReason.set('abc');
        component.reject();
        expect(component.rejecting()).toBe(payment);
        expect(component.rejectError()).toBe('Give a reason of at least 3 characters.');
    });

    it('verify calls the service and reloads', () => {
        component.verify(payment);
        expect(confirm.confirm).toHaveBeenCalled();
        expect(financial.verifyPayment).toHaveBeenCalledWith(7);
        expect(notify.success).toHaveBeenCalledWith(expect.stringContaining('Due #3'));
        expect(financial.getPendingPayments).toHaveBeenCalledTimes(2);
    });

    it('verify says no due was settled when dueId is null', () => {
        financial.verifyPayment.mockReturnValue(of({ dueId: null }));
        component.verify(payment);
        expect(notify.success).toHaveBeenCalledWith(expect.stringContaining('No due settled'));
    });

    it('verify does nothing when the admin cancels', () => {
        confirm.confirm.mockReturnValue(of(false));
        component.verify(payment);
        expect(financial.verifyPayment).not.toHaveBeenCalled();
    });

    it('409 shows the message and reloads', () => {
        financial.verifyPayment.mockReturnValue(throwError(() => new HttpErrorResponse({
            status: 409, error: { detail: 'This payment has already been reviewed.' },
        })));
        component.verify(payment);
        expect(notify.error).toHaveBeenCalledWith('This payment has already been reviewed.');
        expect(financial.getPendingPayments).toHaveBeenCalledTimes(2);
        expect(component.busyId()).toBeNull();
    });

    it('opens the tab before loading the proof, then points it at the object URL', () => {
        const create = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:proof');
        const tab = { opener: {}, location: { href: '' }, close: vi.fn() };
        const open = vi.spyOn(window, 'open').mockReturnValue(tab as unknown as Window);
        component.openProof(payment);
        expect(open).toHaveBeenCalledWith('', '_blank');
        expect(financial.getProof).toHaveBeenCalledWith('secure_uploads/12/proof.jpg');
        expect(tab.opener).toBeNull();
        expect(tab.location.href).toBe('blob:proof');
        create.mockRestore();
        open.mockRestore();
    });

    it('says so when the browser blocks the proof tab', () => {
        const open = vi.spyOn(window, 'open').mockReturnValue(null);
        component.openProof(payment);
        expect(financial.getProof).not.toHaveBeenCalled();
        expect(notify.error).toHaveBeenCalledWith(expect.stringContaining('blocked'));
        open.mockRestore();
    });

    it('closes the proof tab when the proof fails to load', () => {
        financial.getProof.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
        const tab = { opener: {}, location: { href: '' }, close: vi.fn() };
        const open = vi.spyOn(window, 'open').mockReturnValue(tab as unknown as Window);
        component.openProof(payment);
        expect(tab.close).toHaveBeenCalled();
        expect(notify.error).toHaveBeenCalledWith('Could not load the proof. Please try again.');
        open.mockRestore();
    });

    it('a network failure on reject keeps the dialog and the typed reason', () => {
        financial.rejectPayment.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 0 })));
        component.openReject(payment);
        component.rejectReason.set('Wrong reference');
        component.reject();
        expect(component.rejecting()).toBe(payment);
        expect(component.rejectReason()).toBe('Wrong reference');
        expect(component.rejectError()).toBeTruthy();
        expect(component.busyId()).toBeNull();
    });

    it('a 409 on reject closes the dialog and reloads', () => {
        financial.rejectPayment.mockReturnValue(throwError(() => new HttpErrorResponse({
            status: 409, error: { detail: 'This payment has already been reviewed.' },
        })));
        component.openReject(payment);
        component.rejectReason.set('abc');
        component.reject();
        expect(component.rejecting()).toBeNull();
        expect(notify.error).toHaveBeenCalledWith('This payment has already been reviewed.');
        expect(financial.getPendingPayments).toHaveBeenCalledTimes(2);
    });

    it('the verify question leaves the reference out when there is none', () => {
        component.verify({ ...payment, transactionReference: null });
        const message: string = confirm.confirm.mock.calls[0][0].message;
        expect(message).not.toContain('reference');
        expect(message).toContain('1,000');
    });
});
