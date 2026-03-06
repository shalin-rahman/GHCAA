import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AdminCommService } from './admin-comm.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('AdminCommService', () => {
    let service: AdminCommService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AdminCommService]
        });
        service = TestBed.inject(AdminCommService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should fetch email logs', () => {
        const dummyLogs = [{ id: 1, recipientEmail: 'test@e.com' }];
        service.getLogs(50).subscribe(logs => {
            expect(logs.length).toBe(1);
        });

        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.COMMUNICATION}/logs?count=50`);
        expect(req.request.method).toBe('GET');
        req.flush(dummyLogs);
    });

    it('should fetch templates', () => {
        const dummyTemplates = [{ id: 1, code: 'WELCOME' }];
        service.getTemplates().subscribe(t => expect(t.length).toBe(1));

        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.COMMUNICATION}/templates`);
        expect(req.request.method).toBe('GET');
        req.flush(dummyTemplates);
    });

    it('should handle batch send', () => {
        const dto = { templateCode: 'T1' };
        service.sendBatch(dto).subscribe(r => expect(r).toBeTruthy());

        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.COMMUNICATION}/send-batch`);
        expect(req.request.method).toBe('POST');
        req.flush({});
    });

    it('should handle custom message send', () => {
        const dto = { body: 'Hello' };
        service.sendCustom(dto).subscribe(r => expect(r).toBeTruthy());

        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.COMMUNICATION}/send-custom`);
        expect(req.request.method).toBe('POST');
        req.flush({});
    });

    it('should handle template update', () => {
        const template = { id: 1, code: 'C1', subject: 'S', body: 'B', description: 'D' };
        service.saveTemplate(template).subscribe(r => expect(r).toBeTruthy());

        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.COMMUNICATION}/templates/1`);
        expect(req.request.method).toBe('PUT');
        req.flush({});
    });
});
