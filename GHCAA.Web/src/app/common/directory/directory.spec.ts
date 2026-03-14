import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Directory } from './directory';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';

describe('Directory Component', () => {
    let component: Directory;
    let fixture: ComponentFixture<Directory>;
    let networkServiceMock: any;
    let notificationServiceMock: any;
    let routerMock: any;

    beforeEach(async () => {
        networkServiceMock = {
            searchMembers: vi.fn().mockReturnValue(of([]))
        };
        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };
        routerMock = {
            navigate: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [Directory],
            providers: [
                { provide: NetworkingService, useValue: networkServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: Router, useValue: routerMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Directory);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should search on init', () => {
        expect(networkServiceMock.searchMembers).toHaveBeenCalled();
    });
});
