import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminDashboard } from './admin-dashboard';
import { AdminService } from '../../core/services/admin.service';
import { of } from 'rxjs';

describe('AdminDashboard Component', () => {
    let component: AdminDashboard;
    let fixture: ComponentFixture<AdminDashboard>;
    let adminServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getStats: vi.fn().mockReturnValue(of({ totalMembers: 0, pendingMembers: 0, totalIncome: 0 })),
            getDailyActivity: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [AdminDashboard],
            providers: [
                { provide: AdminService, useValue: adminServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminDashboard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load stats on init', () => {
        expect(adminServiceMock.getStats).toHaveBeenCalled();
    });
});
