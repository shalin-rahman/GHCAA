import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminPaymentConfig } from './admin-payment-config';
import { PaymentConfigService } from '../../core/services/payment-config.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

describe('AdminPaymentConfig Component', () => {
    let component: AdminPaymentConfig;
    let fixture: ComponentFixture<AdminPaymentConfig>;
    let paymentConfigServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        paymentConfigServiceMock = {
            getAllConfigs: vi.fn().mockReturnValue(of([])),
            seedDefaults: vi.fn().mockReturnValue(of({ success: true })),
            toggleConfig: vi.fn().mockReturnValue(of({ success: true })),
            deleteConfig: vi.fn().mockReturnValue(of({ success: true })),
            updateConfig: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminPaymentConfig, ReactiveFormsModule],
            providers: [
                { provide: PaymentConfigService, useValue: paymentConfigServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminPaymentConfig);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load configs on init', () => {
        expect(paymentConfigServiceMock.getAllConfigs).toHaveBeenCalled();
    });
});
