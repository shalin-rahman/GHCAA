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
        service.getApplicableFee('MembershipFee', 'General', '22-03-2024').subscribe(f => {
            expect(f.amount).toBe(1000);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/fees/applicable?category=MembershipFee&type=General&date=22-03-2024`);
        expect(req.request.method).toBe('GET');
        req.flush({ amount: 1000 });
    });

    it('should fetch all fee configs', () => {
        service.getFeeConfigs().subscribe(c => expect(c).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/fees/config`);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should post new fee config', () => {
        const dto = { category: 'Registration', amount: 500 };
        service.addFeeConfig(dto).subscribe(c => expect(c).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/fees/config`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(dto);
        req.flush({ id: 1 });
    });

    it('should put updated fee config', () => {
        const dto = { id: 5, amount: 600 };
        service.updateFeeConfig(dto).subscribe(c => expect(c).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/fees/config`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(dto);
        req.flush({ id: 5 });
    });

    it('should fetch pending payments', () => {
        service.getPendingPayments().subscribe(p => expect(p.length).toBe(1));
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/pending`);
        expect(req.request.method).toBe('GET');
        req.flush([{ id: 7, amount: 500, transactionReference: 'TX1', submittedAt: '2026-11-27' }]);
    });

    it('should verify a payment and return the settled due', () => {
        service.verifyPayment(7).subscribe(r => expect(r.dueId).toBeNull());
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/7/verify`);
        expect(req.request.method).toBe('POST');
        req.flush({ dueId: null });
    });

    it('should reject a payment with the reason in the body', () => {
        service.rejectPayment(7, 'Wrong reference').subscribe();
        const req = httpMock.expectOne(`${API_ENDPOINTS.FINANCIALS}/7/reject`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ reason: 'Wrong reference' });
        req.flush(null, { status: 204, statusText: 'No Content' });
    });

    it('should fetch a proof through the secure files route as a blob', () => {
        service.getProof('/secure_uploads/12/proofs/a b.jpg').subscribe();
        const req = httpMock.expectOne(`${API_ENDPOINTS.SECURE_FILES}/secure_uploads/12/proofs/a%20b.jpg`);
        expect(req.request.method).toBe('GET');
        expect(req.request.responseType).toBe('blob');
        req.flush(new Blob(['x']));
    });
});

