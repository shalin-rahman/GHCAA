import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FinancialService } from './financial.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('FinancialService', () => {
    let service: FinancialService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [FinancialService]
        });
        service = TestBed.inject(FinancialService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should fetch my dues', () => {
        service.getMyDues().subscribe(d => expect(d).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/my-dues`);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });
});
