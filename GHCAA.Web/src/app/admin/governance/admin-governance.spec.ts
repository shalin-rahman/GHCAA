import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminGovernance } from './admin-governance';
import { NetworkingService } from '../../core/services/networking.service';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminGovernance Component', () => {
    let component: AdminGovernance;
    let fixture: ComponentFixture<AdminGovernance>;
    let networkServiceMock: any;
    let adminServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        networkServiceMock = {
            getPeriods: vi.fn().mockReturnValue(of([])),
            getCommittee: vi.fn().mockReturnValue(of([]))
        };
        adminServiceMock = {
            assignECRole: vi.fn().mockReturnValue(of({ success: true })),
            updatePeriod: vi.fn().mockReturnValue(of({ success: true }))
        };
        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminGovernance],
            providers: [
                { provide: NetworkingService, useValue: networkServiceMock },
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminGovernance);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load periods and committee on init', () => {
        expect(networkServiceMock.getPeriods).toHaveBeenCalled();
        expect(networkServiceMock.getCommittee).toHaveBeenCalled();
    });
});
