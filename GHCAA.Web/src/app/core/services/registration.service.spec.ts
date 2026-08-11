import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RegistrationService } from './registration.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('RegistrationService', () => {
    let service: RegistrationService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [RegistrationService]
        });
        service = TestBed.inject(RegistrationService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should call register with formData', () => {
        const formData = new FormData();
        formData.append('fullName', 'Test');
        
        service.register(formData).subscribe(res => {
            expect(res.memberId).toBe(100);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.REGISTER);
        expect(req.request.method).toBe('POST');
        req.flush({ memberId: 100 });
    });

    it('should call verifyEmail', () => {
        service.verifyEmail('test@test.com', '123456').subscribe(res => {
            expect(res.success).toBe(true);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.VERIFY_EMAIL);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ email: 'test@test.com', otpCode: '123456' });
        req.flush({ success: true });
    });

    it('should call getStatus', () => {
        service.getStatus(100, 'test@test.com').subscribe(res => {
            expect(res.status).toBe('Applied');
        });

        // Angular's HttpParams leaves '@' unencoded (it's an allowed query char), so the
        // request URL carries a raw '@', not '%40'.
        const req = httpMock.expectOne(`${API_ENDPOINTS.AUTH.STATUS}/100?email=test@test.com`);
        expect(req.request.method).toBe('GET');
        req.flush({ status: 'Applied' });
    });

    it('should call getPublicPaymentConfigs', () => {
        service.getPublicPaymentConfigs().subscribe(res => {
            expect(res.length).toBe(1);
            expect(res[0].displayName).toBe('bKash');
        });

        const req = httpMock.expectOne(API_ENDPOINTS.PAYMENT_CONFIG.PUBLIC);
        expect(req.request.method).toBe('GET');
        req.flush([{ displayName: 'bKash' }]);
    });
});
