import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminGallery } from './admin-gallery';
import { GalleryService } from '../../core/services/gallery.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminGallery Component', () => {
    let component: AdminGallery;
    let fixture: ComponentFixture<AdminGallery>;
    let galleryServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        galleryServiceMock = {
            getAllGalleries: vi.fn().mockReturnValue(of([])),
            createGallery: vi.fn().mockReturnValue(of({ success: true })),
            toggleActive: vi.fn().mockReturnValue(of({ isActive: true })),
            toggleFeatured: vi.fn().mockReturnValue(of({ isFeatured: true })),
            deleteGallery: vi.fn().mockReturnValue(of({ success: true })),
            uploadPhoto: vi.fn().mockReturnValue(of({ path: 'test.jpg' })),
            addPhotos: vi.fn().mockReturnValue(of({ success: true })),
            removePhoto: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminGallery],
            providers: [
                { provide: GalleryService, useValue: galleryServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminGallery);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load galleries on init', () => {
        expect(galleryServiceMock.getAllGalleries).toHaveBeenCalled();
    });
});
