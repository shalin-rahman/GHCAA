import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GalleryService } from './gallery.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('GalleryService', () => {
    let service: GalleryService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [GalleryService]
        });
        service = TestBed.inject(GalleryService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get all galleries for admin', () => {
        service.getAllGalleries().subscribe(g => expect(g).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.GALLERY}/all`);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should toggle active status', () => {
        service.toggleActive(1).subscribe(res => expect(res.isActive).toBe(true));
        const req = httpMock.expectOne(`${API_ENDPOINTS.GALLERY}/admin/1/toggle-active`);
        expect(req.request.method).toBe('PATCH');
        req.flush({ isActive: true });
    });

    it('should create gallery', () => {
        const mockGallery = { title: 'New' };
        service.createGallery(mockGallery).subscribe(g => expect(g.title).toBe('New'));
        const req = httpMock.expectOne(`${API_ENDPOINTS.GALLERY}/admin`);
        expect(req.request.method).toBe('POST');
        req.flush({ id: 1, title: 'New', isActive: true, photos: [] });
    });
});
