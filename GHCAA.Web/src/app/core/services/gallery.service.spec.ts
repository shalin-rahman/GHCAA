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

    it('should get galleries', () => {
        service.getGalleries().subscribe(g => expect(g).toBeTruthy());
        const req = httpMock.expectOne(API_ENDPOINTS.GALLERY);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });
});
