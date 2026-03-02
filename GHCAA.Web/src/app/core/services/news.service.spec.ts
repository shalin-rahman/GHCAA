import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NewsService } from './news.service';
import { API_ENDPOINTS } from '../constants/api.endpoints';

describe('NewsService', () => {
    let service: NewsService;
    let httpMock: HttpTestingController;

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

    it('should fetch news', () => {
        const dummyNews = [{ id: 1, title: 'News 1' }];
        service.getNews().subscribe(news => {
            expect(news.length).toBe(1);
            expect(news).toEqual(dummyNews as any);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.NEWS);
        expect(req.request.method).toBe('GET');
        req.flush(dummyNews);
    });
});
