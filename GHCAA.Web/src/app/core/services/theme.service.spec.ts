import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ThemeService } from './theme.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ThemeService', () => {
    let service: ThemeService;
    let httpMock: HttpTestingController;

    beforeAll(() => {
        Object.defineProperty(window, 'matchMedia', {
            writable: true,
            value: vi.fn().mockImplementation(query => ({
                matches: false,
                media: query,
                onchange: null,
                addListener: vi.fn(), 
                removeListener: vi.fn(),
                addEventListener: vi.fn(),
                removeEventListener: vi.fn(),
                dispatchEvent: vi.fn(),
            })),
        });
    });

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ThemeService]
        });
        service = TestBed.inject(ThemeService);
        httpMock = TestBed.inject(HttpTestingController);

        // The constructor's loadActiveSpecialTheme() call is deferred via afterNextRender (same
        // NG0200 guard as AuthService) — tick the ApplicationRef so it fires before we expect it.
        TestBed.inject(ApplicationRef).tick();
        const req = httpMock.expectOne(`${API_ENDPOINTS.THEMES}/active`);
        req.flush({});
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
