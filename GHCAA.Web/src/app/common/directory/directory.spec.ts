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
        vi.useFakeTimers();

        networkServiceMock = {
            searchMembers: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 20, hasNextPage: false }))
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

    afterEach(() => {
        vi.restoreAllMocks();
        vi.useRealTimers();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should search on init', () => {
        vi.advanceTimersByTime(300); // flush the 300ms debounce
        expect(networkServiceMock.searchMembers).toHaveBeenCalled();
    });
});

