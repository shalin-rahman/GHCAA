import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminDashboard } from './admin-dashboard';
import { AdminService } from '../../core/services/admin.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';

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
                { provide: AdminService, useValue: adminServiceMock },
                provideRouter([])
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

    describe('safeImg', () => {
        it('passes through a real absolute path', () => {
            expect(component.safeImg({ imageUrl: '/uploads/x.png' })).toBe('/uploads/x.png');
        });

        it('passes through a real full URL, preferring coverImageUrl over imageUrl', () => {
            expect(component.safeImg({ coverImageUrl: 'https://cdn.example.com/a.jpg', imageUrl: '/b.png' }))
                .toBe('https://cdn.example.com/a.jpg');
        });

        it('falls back to empty string for bad seed/import data like "..."', () => {
            expect(component.safeImg({ imageUrl: '...' })).toBe('');
        });

        it('falls back to empty string when neither field is set', () => {
            expect(component.safeImg({})).toBe('');
        });
    });

    describe('newsStatus', () => {
        it('maps Pending (1) to a distinct label, not "Live"', () => {
            expect(component.newsStatus({ status: 1 })).toEqual({ label: 'Pending Approval', class: 'pending' });
        });

        it('maps Approved (2) to Approved/active', () => {
            expect(component.newsStatus({ status: 2 })).toEqual({ label: 'Approved', class: 'active' });
        });

        it('maps Draft (0) to Draft', () => {
            expect(component.newsStatus({ status: 0 })).toEqual({ label: 'Draft', class: 'draft' });
        });

        it('defaults to Approved when status is missing', () => {
            expect(component.newsStatus({})).toEqual({ label: 'Approved', class: 'active' });
        });
    });
});
