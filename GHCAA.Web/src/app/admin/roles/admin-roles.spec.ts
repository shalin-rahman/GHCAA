import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminRoles } from './admin-roles';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

describe('AdminRoles Component', () => {
    let component: AdminRoles;
    let fixture: ComponentFixture<AdminRoles>;
    let httpMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        httpMock = {
            get: vi.fn().mockReturnValue(of([])),
            post: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminRoles, ReactiveFormsModule],
            providers: [
                { provide: HttpClient, useValue: httpMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminRoles);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load users and roles on init', () => {
        expect(httpMock.get).toHaveBeenCalledWith('/api/roles/users');
        expect(httpMock.get).toHaveBeenCalledWith('/api/roles');
    });
});
