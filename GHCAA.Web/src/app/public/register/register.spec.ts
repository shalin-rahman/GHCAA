import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Register } from './register';
import { RegistrationService } from '../../core/services/registration.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('Register Component', () => {
    let component: Register;
    let fixture: ComponentFixture<Register>;
    let regServiceMock: any;
    let notificationServiceMock: any;
    let routerMock: any;

    beforeEach(async () => {
        regServiceMock = {
            getPublicPaymentConfigs: vi.fn().mockReturnValue(of([])),
            register: vi.fn(),
            verifyEmail: vi.fn()
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        routerMock = {
            navigate: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [Register],
            providers: [
                { provide: RegistrationService, useValue: regServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: Router, useValue: routerMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Register);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should navigate between steps', () => {
        expect(component.currentStep()).toBe(1);
        component.nextStep();
        expect(component.currentStep()).toBe(2);
        component.prevStep();
        expect(component.currentStep()).toBe(1);
    });

    it('should add/remove academic history', () => {
        const initialCount = component.model.AcademicHistory.length;
        component.addAcademic();
        expect(component.model.AcademicHistory.length).toBe(initialCount + 1);
        component.removeAcademic(initialCount);
        expect(component.model.AcademicHistory.length).toBe(initialCount);
    });
});
