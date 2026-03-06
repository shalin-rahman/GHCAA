import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AdminService } from './admin.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('AdminService', () => {
    let service: AdminService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AdminService]
        });
        service = TestBed.inject(AdminService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should fetch pending members', () => {
        service.getPendingMembers().subscribe(a => expect(a).toBeTruthy());
        const req = httpMock.expectOne(API_ENDPOINTS.ADMIN.MEMBERS);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });
});
