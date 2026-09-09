import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NewsService } from './news.service';
import { NewsPost } from '../models/business.models';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('NewsService', () => {
    let service: NewsService;
    let httpMock: HttpTestingController;

    const mockPost: NewsPost = {
        id: 1,
        title: 'Tech Update',
        content: 'Latest tech news.',
        articleCategory: 'Regular',
        postType: 'Article' as any,
        status: 'Approved',
        isActive: true,
        authorName: 'Admin',
        createdAt: '08-03-2026',
        collaborators: []
    };


    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [NewsService]
        });
        service = TestBed.inject(NewsService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get active news', () => {
        service.getNews().subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.NEWS);
        expect(req.request.method).toBe('GET');
        req.flush([mockPost]);
    });

    it('should send article category as a query parameter', () => {
        service.getNews('Magazine').subscribe();
        const req = httpMock.expectOne(`${API_ENDPOINTS.NEWS}?articleCategory=Magazine`);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should get news silently with special header', () => {
        service.getNews(undefined, true).subscribe();
        const req = httpMock.expectOne(API_ENDPOINTS.NEWS);
        expect(req.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        req.flush([]);
    });

    it('should get news by id', () => {
        service.getNewsById(1).subscribe(res => {
            expect(res.title).toBe('Tech Update');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.NEWS}/1`);
        expect(req.request.method).toBe('GET');
        req.flush(mockPost);
    });

    it('should get all news for admin', () => {
        service.getNewsAdmin().subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.NEWS}/admin`);
        expect(req.request.method).toBe('GET');
        req.flush([mockPost]);
    });

    it('should create news', () => {
        const payload = { title: 'New', content: 'C', articleCategory: 'Regular' as any, isActive: true };
        service.createNews(payload).subscribe(res => {
            expect(res.id).toBe(1);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.NEWS);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(payload);
        req.flush(mockPost);
    });

    it('should update news', () => {
        const payload = { id: 1, title: 'Updated' };
        service.updateNews(1, payload).subscribe(res => {
            expect(res.id).toBe(1);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.NEWS}/1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(payload);
        req.flush(mockPost);
    });

    it('should delete news', () => {
        service.deleteNews(1).subscribe();
        const req = httpMock.expectOne(`${API_ENDPOINTS.NEWS}/1`);
        expect(req.request.method).toBe('DELETE');
        req.flush(null);
    });
});
