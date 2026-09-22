import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ArchiveService } from './archive.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ArchiveService', () => {
    let service: ArchiveService;
    let http: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [ArchiveService, provideHttpClient(), provideHttpClientTesting()]
        });
        service = TestBed.inject(ArchiveService);
        http = TestBed.inject(HttpTestingController);
    });

    afterEach(() => http.verify());

    it('loads public collections with the optional search term', () => {
        service.getPublicCollections('  stories  ').subscribe();
        const request = http.expectOne(r => r.url === `${API_ENDPOINTS.ARCHIVE}/public`);
        expect(request.request.method).toBe('GET');
        expect(request.request.params.get('search')).toBe('stories');
        request.flush([]);
    });

    it('loads a public archive item by id', () => {
        service.getPublicItem(7).subscribe();
        const request = http.expectOne(`${API_ENDPOINTS.ARCHIVE}/items/7`);
        expect(request.request.method).toBe('GET');
        request.flush({ id: 7 });
    });
});
