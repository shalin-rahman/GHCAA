import { ComponentFixture, TestBed } from '@angular/core/testing';
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
            getToken: vi.fn().mockReturnValue(null)
        };

        const notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

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
