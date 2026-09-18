import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminDevTracker } from './admin-dev-tracker';
import { AdminService } from '../../core/services/admin.service';
import { of, throwError } from 'rxjs';

describe('AdminDevTracker Component', () => {
    let component: AdminDevTracker;
    let fixture: ComponentFixture<AdminDevTracker>;
    let adminServiceMock: any;

    const sampleItems = [
        { id: '82.1', status: 'TODO', priority: 'P2', dependsOn: null, summary: 'First open item.', workPackageNumber: 82, workPackageTitle: 'Sample batch' },
        { id: '82.3', status: 'PARTIAL', priority: 'P3', dependsOn: '82.1', summary: 'Half-finished item.', workPackageNumber: 82, workPackageTitle: 'Sample batch' },
        { id: '6.2', status: 'TODO', priority: 'P3', dependsOn: null, summary: 'Legacy item.', workPackageNumber: 6, workPackageTitle: 'Legacy batch' }
    ];

    beforeEach(async () => {
        adminServiceMock = {
            getDevTrackerItems: vi.fn().mockReturnValue(of(sampleItems))
        };

        await TestBed.configureTestingModule({
            imports: [AdminDevTracker],
            providers: [{ provide: AdminService, useValue: adminServiceMock }]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminDevTracker);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load tracker items on init', () => {
        expect(adminServiceMock.getDevTrackerItems).toHaveBeenCalledWith('all');
        expect(component.items().length).toBe(3);
    });

    it('should group items by work package', () => {
        const groups = component.groups();
        expect(groups.length).toBe(2);
        expect(groups.find(g => g.workPackageNumber === 82)?.items.length).toBe(2);
        expect(groups.find(g => g.workPackageNumber === 6)?.items.length).toBe(1);
    });

    it('should reload with the selected priority when the filter changes', () => {
        component.priorityFilter.set('P1');
        component.onFilterChange();
        expect(adminServiceMock.getDevTrackerItems).toHaveBeenCalledWith('P1');
    });

    it('should map PARTIAL and TODO to the existing badge state classes', () => {
        expect(component.getStatusClass('PARTIAL')).toBe('pending');
        expect(component.getStatusClass('TODO')).toBe('terminated');
    });

    it('should clear the list and notify on load failure', () => {
        adminServiceMock.getDevTrackerItems.mockReturnValue(throwError(() => ({ error: { detail: 'boom' } })));
        component.loadItems();
        expect(component.items()).toEqual([]);
        expect(component.groups()).toEqual([]);
        expect(component.loading()).toBe(false);
    });
});
