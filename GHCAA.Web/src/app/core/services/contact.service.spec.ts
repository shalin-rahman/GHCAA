import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ContactService } from './contact.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ContactService', () => {
    let service: ContactService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ContactService]
        });
        service = TestBed.inject(ContactService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should send message', () => {
        const msg = { fullName: 'Test', email: 'test@t.com', subject: 'hello', message: 'world' };
        service.sendMessage(msg).subscribe(res => {
            expect(res.success).toBe(true);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.CONTACT);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(msg);
        req.flush({ success: true });
    });
});
