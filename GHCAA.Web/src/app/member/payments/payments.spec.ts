import { ComponentFixture, TestBed } from '@angular/core/testing';
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
            recordPayment: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

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
});
