import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Payments } from './payments';
import { FinancialService } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('Payments Component', () => {
    let component: Payments;
    let fixture: ComponentFixture<Payments>;
    let financialServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        financialServiceMock = {
            getMyDues: vi.fn().mockReturnValue(of([])),
            getMyHistory: vi.fn().mockReturnValue(of([])),
            getSavedMethods: vi.fn().mockReturnValue(of([])),
            recordPayment: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [Payments],
            providers: [
                { provide: FinancialService, useValue: financialServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Payments);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load dues and history on init', () => {
        expect(financialServiceMock.getMyDues).toHaveBeenCalled();
        expect(financialServiceMock.getMyHistory).toHaveBeenCalled();
    });

    it('should map enum payment statuses from the backend', () => {
        expect(component.getStatusLabel('Pending')).toBe('Pending Verification');
        expect(component.getStatusClass('Completed')).toBe('success');
        expect(component.getStatusLabel('Failed')).toBe('Failed');
        expect(component.getStatusLabel('Refunded')).toBe('Refunded');
    });

    it('should format financial category labels', () => {
        expect(component.getCategoryLabel('MembershipFee')).toBe('Yearly Membership Fee');
        expect(component.getCategoryLabel('Utilities')).toBe('Utilities');
    });

    it('shows the rejection reason', () => {
        financialServiceMock.getMyHistory.mockReturnValue(of([
            { id: 1, transactionId: 'TX-1', amount: 500, paidAt: '2026-11-20', status: 'Failed', financialCategory: 'MembershipFee', notes: 'Annual dues', rejectionReason: 'Reference not found | bank checked' },
            { id: 2, transactionId: 'TX-2', amount: 500, paidAt: '2026-11-21', status: 'Pending', financialCategory: 'MembershipFee', notes: 'Annual dues' },
            { id: 3, transactionId: 'TX-3', amount: 500, paidAt: '2026-11-22', status: 'Failed', financialCategory: 'MembershipFee', notes: 'Annual dues | gateway timeout', rejectionReason: null },
        ]));
        component.ngOnInit();
        fixture.detectChanges();
        const reasons = fixture.nativeElement.querySelectorAll('.rejection-reason');
        expect(reasons.length).toBe(1);
        expect(reasons[0].textContent).toContain('Reason: Reference not found | bank checked');
    });
});
