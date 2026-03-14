import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ThemeService } from './theme.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ThemeService', () => {
    let service: ThemeService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ThemeService]
        });
        service = TestBed.inject(ThemeService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should toggle theme', () => {
        const initial = service.theme();
        service.toggleTheme();
        expect(service.theme()).not.toBe(initial);
    });

    it('should load active special theme', () => {
        const mockTheme = { title: 'Eid' };
        service.loadActiveSpecialTheme();
        const req = httpMock.expectOne(`${API_ENDPOINTS.THEMES}/active`);
        expect(req.request.method).toBe('GET');
        req.flush(mockTheme);
        expect(service.activeSpecialTheme()?.title).toBe('Eid');
    });
});
