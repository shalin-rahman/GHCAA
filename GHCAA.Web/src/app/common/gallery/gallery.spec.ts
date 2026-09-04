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
            getGalleries: vi.fn().mockReturnValue(of([]))
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

    it('should load galleries on init', () => {
        expect(galleryServiceMock.getGalleries).toHaveBeenCalled();
    });
});
