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
        service.getPendingMembers(1, 10, 'search').subscribe(res => {
            expect(res).toBeTruthy();
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}?page=1&pageSize=10&statusFilter=Applied&searchQuery=search`);
        expect(req.request.method).toBe('GET');
        req.flush({ items: [] });
    });

    it('should fetch members', () => {
        service.getMembers(2, 20, 'test', 'Active', true).subscribe(res => {
            expect(res).toBeTruthy();
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}?page=2&pageSize=20&includeArchived=true&statusFilter=Active&searchQuery=test`);
        expect(req.request.method).toBe('GET');
        req.flush({ items: [] });
    });

    it('should approve member', () => {
        service.approveMember(1, 100).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/1/approve`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ approvedByAdminId: 100 });
        req.flush({ success: true });
    });

    it('should reject member', () => {
        service.rejectMember(2, 100, 'spam').subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/2/reject`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ rejectedByAdminId: 100, reason: 'spam' });
        req.flush({ success: true });
    });

    it('should get stats', () => {
        service.getStats().subscribe(res => {
            expect(res.totalMembers).toBe(5);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.ADMIN.STATS);
        expect(req.request.method).toBe('GET');
        req.flush({ totalMembers: 5, applied: 1, active: 4, inactive: 0, balance: 1000, lastUpdated: new Date().toISOString() });
    });


    it('should archive member', () => {
        service.archiveMember(5).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/5`);
        expect(req.request.method).toBe('DELETE');
        req.flush({ success: true });
    });

    it('should reactivate member', () => {
        service.reactivateMember(5).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/5/reactivate`);
        expect(req.request.method).toBe('POST');
        req.flush({ success: true });
    });

    it('should update member', () => {
        const payload = { fullName: 'Updated Name' };
        service.updateMember(10, payload).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/10`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(payload);
        req.flush({ success: true });
    });

    it('should send password reset link', () => {
        service.sendPasswordResetLink(15).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.ADMIN.MEMBERS}/15/reset-password-admin`);
        expect(req.request.method).toBe('POST');
        req.flush({ success: true });
    });

    it('should import members', () => {
        const formData = new FormData();
        formData.append('file', 'test');
        service.importMembers(formData).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.ADMIN.MEMBERS_IMPORT);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toBe(formData);
        req.flush({ success: true });
    });
});
