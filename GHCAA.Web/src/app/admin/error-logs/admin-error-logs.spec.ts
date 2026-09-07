import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminErrorLogs } from './admin-error-logs';
import { AdminService } from '../../core/services/admin.service';
import { of } from 'rxjs';

describe('AdminErrorLogs Component', () => {
    let component: AdminErrorLogs;
    let fixture: ComponentFixture<AdminErrorLogs>;
    let adminServiceMock: any;

    const sampleLog = {
        id: 1,
        occurredAt: '2026-09-07T10:00:00Z',
        level: 'Error',
        message: 'Null reference in profile save',
        exceptionType: 'System.NullReferenceException',
        stackTrace: 'at Foo.Bar()',
        source: 'ExceptionMiddleware',
        requestPath: '/api/members',
        requestMethod: 'GET',
        username: 'shalin'
    };

    beforeEach(async () => {
        adminServiceMock = {
            getErrorLogs: vi.fn().mockReturnValue(of({ items: [sampleLog], totalItems: 1, totalPages: 1 }))
        };

        await TestBed.configureTestingModule({
            imports: [AdminErrorLogs],
            providers: [{ provide: AdminService, useValue: adminServiceMock }]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminErrorLogs);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load error logs on init', () => {
        expect(adminServiceMock.getErrorLogs).toHaveBeenCalled();
        expect(component.logs().length).toBe(1);
    });

    it('should reset to page 1 and reload when a filter changes', () => {
        component.currentPage.set(3);
        component.levelFilter.set('Warning');
        component.onFilterChange();
        expect(component.currentPage()).toBe(1);
        expect(adminServiceMock.getErrorLogs).toHaveBeenCalledTimes(2);
    });

    it('should toggle stack trace expansion for a row', () => {
        expect(component.expandedId()).toBeNull();
        component.toggleExpand(1);
        expect(component.expandedId()).toBe(1);
        component.toggleExpand(1);
        expect(component.expandedId()).toBeNull();
    });

    it('should map Error and Warning to the existing badge state classes', () => {
        expect(component.getLevelClass('Error')).toBe('terminated');
        expect(component.getLevelClass('Warning')).toBe('pending');
    });
});
