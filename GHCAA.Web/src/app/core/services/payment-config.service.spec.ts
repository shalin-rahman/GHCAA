import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaymentConfigService } from './payment-config.service';

describe('PaymentConfigService', () => {
    let service: PaymentConfigService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [PaymentConfigService]
        });
        service = TestBed.inject(PaymentConfigService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get active payment methods', () => {
        service.getActivePaymentMethods().subscribe(res => {
            expect(res).toEqual([]);
        });
        const req = httpMock.expectOne('/api/payment-config/active');
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should get all configs for admin', () => {
        service.getAllConfigs().subscribe(res => {
            expect(res).toEqual([]);
        });
        const req = httpMock.expectOne('/api/payment-config/admin/all');
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should create config', () => {
        const config = { method: 'Bkash' };
        service.createConfig(config).subscribe(res => {
            expect(res).toBeTruthy();
        });
        const req = httpMock.expectOne('/api/payment-config/admin');
        expect(req.request.method).toBe('POST');
        req.flush({ success: true });
    });
});
