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
    it('should fetch applicable fee dynamically', () => {
        service.getApplicableFee('MembershipFee', 'General', '2024-03-22').subscribe(f => {
            expect(f.amount).toBe(1000);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/fees/applicable?category=MembershipFee&type=General&date=2024-03-22`);
        expect(req.request.method).toBe('GET');
        req.flush({ amount: 1000 });
    });
});
