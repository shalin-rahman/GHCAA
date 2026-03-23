import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminFeeConfig } from './admin-fee-config';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FinancialService } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

describe('AdminFeeConfig Component', () => {
    let component: AdminFeeConfig;
    let fixture: ComponentFixture<AdminFeeConfig>;
    let financialServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        financialServiceMock = {
            getFeeConfigs: vi.fn().mockReturnValue(of([])),
            addFeeConfig: vi.fn().mockReturnValue(of({})),
            updateFeeConfig: vi.fn().mockReturnValue(of({}))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            info: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminFeeConfig, HttpClientTestingModule, ReactiveFormsModule],
            providers: [
                { provide: FinancialService, useValue: financialServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminFeeConfig);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load configs on init', () => {
        expect(financialServiceMock.getFeeConfigs).toHaveBeenCalled();
    });

    it('should open create form and reset values', () => {
        component.openCreateForm();
        expect(component.showForm()).toBe(true);
        expect(component.editingId()).toBeNull();
        expect(component.form.value.category).toBe('RegistrationFee');
    });

    it('should open edit form with config data', () => {
        const config = { id: 1, category: 'MembershipFee', amount: 500, effectiveDate: '2023-01-01' };
        component.openEditForm(config);
        expect(component.showForm()).toBe(true);
        expect(component.editingId()).toBe(1);
        expect(component.form.value.amount).toBe(500);
    });

    it('should call addFeeConfig on valid new submission', () => {
        component.openCreateForm();
        component.form.patchValue({ amount: 1000, effectiveDate: '2023-12-31' });
        
        component.submitForm();
        
        expect(financialServiceMock.addFeeConfig).toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledWith('New configuration added');
    });

    it('should call updateFeeConfig on valid edit submission', () => {
        const config = { id: 5, category: 'MembershipFee', amount: 500, effectiveDate: '2023-01-01' };
        component.openEditForm(config);
        component.form.patchValue({ amount: 600 });
        
        component.submitForm();
        
        expect(financialServiceMock.updateFeeConfig).toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Configuration updated');
    });

    it('should handle error during submission', () => {
        financialServiceMock.addFeeConfig.mockReturnValue(throwError(() => ({ error: { message: 'Api Error' } })));
        component.openCreateForm();
        component.form.patchValue({ amount: 1000, effectiveDate: '2023-12-31' });
        
        component.submitForm();
        
        expect(notificationServiceMock.error).toHaveBeenCalledWith('Api Error');
    });

    it('should show info on delete attempt', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        component.deleteConfig(1);
        expect(notificationServiceMock.info).toHaveBeenCalled();
    });
});
