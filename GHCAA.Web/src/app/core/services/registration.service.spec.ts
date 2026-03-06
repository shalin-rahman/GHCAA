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

    it('should submit registration with FormData (including image)', () => {
        const formData = new FormData();
        formData.append('fullName', 'John Doe');

        // Mock a file upload
        const blob = new Blob([''], { type: 'image/jpeg' });
        formData.append('photo', blob, 'test.jpg');

        service.register(formData).subscribe(res => {
            expect(res).toBeTruthy();
            expect(res.memberId).toBe(123);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.REGISTER);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toBe(formData);
        req.flush({ memberId: 123, message: 'Success' });
    });

    it('should verify email with OTP', () => {
        const email = 'test@example.com';
        const otpCode = '123456';

        service.verifyEmail(email, otpCode).subscribe(res => {
            expect(res).toBeTruthy();
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.VERIFY_EMAIL);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ email, otpCode });
        req.flush({ message: 'Verified' });
    });

    it('should get registration status', () => {
        service.getStatus(1).subscribe(l => expect(l).toBeTruthy());
        const req = httpMock.expectOne(`${API_ENDPOINTS.AUTH.STATUS}/1`);
        expect(req.request.method).toBe('GET');
        req.flush({ status: 'Applied' });
    });
});
