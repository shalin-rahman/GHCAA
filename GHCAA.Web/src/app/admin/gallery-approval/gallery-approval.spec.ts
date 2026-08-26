import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GalleryApproval } from './gallery-approval';
import { GalleryService } from '../../core/services/gallery.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('GalleryApproval Component', () => {
    let component: GalleryApproval;
    let fixture: ComponentFixture<GalleryApproval>;
    let galleryServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        galleryServiceMock = {
            getPendingApprovals: vi.fn().mockReturnValue(of({ galleries: [], photos: [] })),
            approveGallery: vi.fn().mockReturnValue(of({ success: true })),
            rejectGallery: vi.fn().mockReturnValue(of({ success: true })),
            approvePhoto: vi.fn().mockReturnValue(of({ success: true })),
            rejectPhoto: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [GalleryApproval],
            providers: [
                { provide: GalleryService, useValue: galleryServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(GalleryApproval);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load pending approvals on init', () => {
        expect(galleryServiceMock.getPendingApprovals).toHaveBeenCalled();
    });

    it('should approve the selected album', () => {
        const album: any = { id: 1, title: 'Test Album', photos: [] };
        component.viewAlbum(album);
        component.approve();
        expect(galleryServiceMock.approveGallery).toHaveBeenCalledWith(1);
    });

    it('should reject the selected photo with a reason', () => {
        const photo: any = { id: 5, photoPath: '/uploads/x.jpg' };
        component.viewPhoto(photo);
        component.rejectReason.set('Not appropriate');
        component.reject();
        expect(galleryServiceMock.rejectPhoto).toHaveBeenCalledWith(5, 'Not appropriate');
    });
});
