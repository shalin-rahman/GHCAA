import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Gallery } from './gallery';
import { GalleryService } from '../../core/services/gallery.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { signal, computed } from '@angular/core';

describe('Gallery Component', () => {
    let component: Gallery;
    let fixture: ComponentFixture<Gallery>;
    let galleryServiceMock: any;

    beforeEach(async () => {
        galleryServiceMock = {
            getGalleriesPage: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 20 }))
        };

        const authServiceMock = {
            isAuthenticated: computed(() => false),
            currentUser: computed(() => null),
            getToken: vi.fn().mockReturnValue(null),
            // Real signal (not vi.fn()) — whenAuthenticated() reads it directly.
            authChecked: signal(true),
            // Mirrors AuthService.whenAuthenticated: a synchronous check-and-call is enough here.
            whenAuthenticated: vi.fn((callback: () => void) => {
                if (authServiceMock.authChecked() && authServiceMock.isAuthenticated()) callback();
            })
        };

        const notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [Gallery],
            providers: [
                { provide: GalleryService, useValue: galleryServiceMock },
                { provide: AuthService, useValue: authServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Gallery);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load the first page of galleries on init', () => {
        expect(galleryServiceMock.getGalleriesPage).toHaveBeenCalledWith(1, 20);
    });

    // NFR-P1: galleries load a page at a time, and a refresh starts again from page 1.
    it('appends the next page and starts over on refresh', () => {
        const page = (ids: number[], p: number) => of({ items: ids.map(id => ({ id, photos: [] })), totalItems: 2, totalPages: 2, page: p, pageSize: 20 });
        galleryServiceMock.getGalleriesPage
            .mockReturnValueOnce(page([1], 1))
            .mockReturnValueOnce(page([2], 2))
            .mockReturnValueOnce(page([1], 1));

        component.refresh();
        component.loadNextPage();
        expect(component.galleries().map(g => g.id)).toEqual([1, 2]);
        expect(component.hasMore()).toBe(false);

        component.refresh();
        expect(galleryServiceMock.getGalleriesPage).toHaveBeenLastCalledWith(1, 20);
        expect(component.galleries().map(g => g.id)).toEqual([1]);
    });
});
