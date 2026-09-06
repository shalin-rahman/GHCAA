import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
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

        notificationServiceMock = createNotificationServiceMock();

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
        expect(galleryServiceMock.approveGallery).toHaveBeenCalledWith(1, true);
    });

    it('should reject the selected photo with a reason', () => {
        const photo: any = { id: 5, photoPath: '/uploads/x.jpg' };
        component.viewPhoto(photo);
        component.rejectReason.set('Not appropriate');
        component.reject();
        expect(galleryServiceMock.rejectPhoto).toHaveBeenCalledWith(5, 'Not appropriate', true);
    });

    // 82.52 batch 3: notifyMember defaults true (matches the unconditional notify these already did)
    // and can be turned off per submission.
    it('viewAlbum resets notifyMember to true and honors opt-out on approve', () => {
        const album: any = { id: 2, title: 'Another Album', photos: [] };
        component.viewAlbum(album);
        expect(component.notifyMember()).toBe(true);
        component.notifyMember.set(false);
        component.approve();
        expect(galleryServiceMock.approveGallery).toHaveBeenCalledWith(2, false);
    });

    it('rejectGallery carries notifyMember:false when the admin opts out', () => {
        const album: any = { id: 3, title: 'Third Album', photos: [] };
        component.viewAlbum(album);
        component.rejectReason.set('Not appropriate');
        component.notifyMember.set(false);
        component.reject();
        expect(galleryServiceMock.rejectGallery).toHaveBeenCalledWith(3, 'Not appropriate', false);
    });
});
