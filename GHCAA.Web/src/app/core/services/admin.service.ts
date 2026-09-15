import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
    MembershipStatus, 
    MembershipType, 
    Gender, 
    BloodGroup,
    AcademicRecord,
    ProfessionalRecord
} from '../models/business.models';


export interface MemberApprovalRequest {
    id: number;
    fullName: string;
    fatherName: string;
    motherName: string;
    dateOfBirth: string;
    gender: Gender;
    bloodGroup: BloodGroup;
    nid: string;
    email: string;
    mobileNo: string;
    presentAddress: string;
    permanentAddress: string;
    hscAdmissionYear?: number;
    highestCertificate: string;
    highestCertificateGroup: string;
    highestCertificateSubject: string;
    highestCertificatePassingYear: number;
    ghcAdmissionYear?: number;
    ghcLastCertificate: string;
    ghcLastCertificateGroup: string;
    ghcLastCertificateSubject: string;
    ghcLastCertificatePassingYear: number;
    professionalSector: string;
    designation: string;
    emergencyContactName: string;
    emergencyContactRelation: string;
    emergencyContactPhone: string;
    membershipType: MembershipType;
    status: MembershipStatus;
    photoPath?: string;
    certificatePath?: string;
    academicHistory?: AcademicRecord[];
    professionalHistory?: ProfessionalRecord[];
}


export interface DashboardStats {
    totalMembers: number;
    applied: number;
    active: number;
    inactive: number;
    // null = caller is not permitted to see it (non-SuperAdmin), which is not the same as 0.
    balance: number | null;
    lastUpdated: string;
}


import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.ADMIN.MEMBERS;

    getPendingMembers(page: number = 1, pageSize: number = 10, searchQuery: string = ''): Observable<any> {
        let params = `?page=${page}&pageSize=${pageSize}&statusFilter=Applied`;
        if (searchQuery) params += `&searchQuery=${encodeURIComponent(searchQuery)}`;
        return this.http.get<any>(`${this.apiUrl}${params}`);
    }

    getMembers(page: number = 1, pageSize: number = 10, searchQuery: string = '', statusFilter: string = 'all', categoryFilter: string = 'all', membershipTypeFilter: string = 'all', includeArchived: boolean = false): Observable<any> {
        let params = `?page=${page}&pageSize=${pageSize}&includeArchived=${includeArchived}&statusFilter=${statusFilter}&categoryFilter=${categoryFilter}&membershipTypeFilter=${membershipTypeFilter}`;
        if (searchQuery) params += `&searchQuery=${encodeURIComponent(searchQuery)}`;
        return this.http.get<any>(`${this.apiUrl}${params}`);
    }

    getMemberById(id: number): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/${id}`);
    }

    getAllForExport(searchQuery: string = '', statusFilter: string = 'all', categoryFilter: string = 'all', membershipTypeFilter: string = 'all'): Observable<any[]> {
        // Fetch a large number or use a specific export endpoint if you add one later
        let params = `?page=1&pageSize=10000&statusFilter=${statusFilter}&categoryFilter=${categoryFilter}&membershipTypeFilter=${membershipTypeFilter}`;
        if (searchQuery) params += `&searchQuery=${encodeURIComponent(searchQuery)}`;
        return this.http.get<any>(`${this.apiUrl}${params}`);
    }

    // 29F.1: The acting admin's identity is taken from the JWT server-side (AdminController reads the
    // MemberId claim and ignores any client-supplied id). The old approvedByAdminId/rejectedByAdminId
    // body fields were dead and misleading (callers hardcoded 1 / fell back to `|| 1`), so they are gone.
    approveMember(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/approve`, {});
    }

    revertMemberApproval(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/revert-approval`, {});
    }

    rejectMember(id: number, reason: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reject`, { reason });
    }

    getStats(): Observable<DashboardStats> {
        return this.http.get<DashboardStats>(API_ENDPOINTS.ADMIN.STATS);
    }

    getPendingSummary(): Observable<any> {
        return this.http.get<any>(API_ENDPOINTS.PENDING.ADMIN_SUMMARY);
    }


    archiveMember(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    restoreMember(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/restore`, {});
    }

    reactivateMember(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reactivate`, {});
    }

    updateMember(id: number, data: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, data);
    }

    updateMemberDocuments(id: number, certificate?: File, paymentProof?: File): Observable<any> {
        const formData = new FormData();
        if (certificate) formData.append('certificate', certificate);
        if (paymentProof) formData.append('paymentProof', paymentProof);
        return this.http.patch(`${this.apiUrl}/${id}/documents`, formData);
    }

    sendPasswordResetLink(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reset-password-admin`, {});
    }

    updateMemberPhoto(id: number, photo: File): Observable<any> {
        const formData = new FormData();
        formData.append('photo', photo);
        return this.http.post<any>(`${this.apiUrl}/${id}/photo`, formData);
    }

    updateMemberSignature(id: number, signature: File): Observable<any> {
        const formData = new FormData();
        formData.append('signature', signature);
        return this.http.post<any>(`${this.apiUrl}/${id}/signature`, formData);
    }

    importMembers(formData: FormData): Observable<any> {
        return this.http.post(API_ENDPOINTS.ADMIN.MEMBERS_IMPORT, formData);
    }

    // Theme Management
    getAllThemes(): Observable<any[]> {
        return this.http.get<any[]>(`${API_ENDPOINTS.THEMES}/all`);
    }

    createTheme(theme: any): Observable<any> {
        return this.http.post(API_ENDPOINTS.THEMES, theme);
    }

    updateTheme(id: number, theme: any): Observable<any> {
        return this.http.put(`${API_ENDPOINTS.THEMES}/${id}`, theme);
    }

    deleteTheme(id: number): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.THEMES}/${id}`);
    }

    // Governance - EC Roles
    deleteECMember(id: number, notifyMember = false): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/members/${id}/hard-delete?notifyMember=${notifyMember}`);
    }

    // Finances - Payments
    getMemberPayments(memberId: number): Observable<any[]> {
        return this.http.get<any[]>(`${API_ENDPOINTS.FINANCIALS}/member/${memberId}/history`);
    }

    deletePayment(id: number): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.FINANCIALS}/payment/${id}`);
    }

    getPeriods(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
    }

    getGovernancePeriods(): Observable<any[]> {
        return this.http.get<any[]>(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods`);
    }

    getCommitteeMembers(periodId: number): Observable<any[]> {
        return this.http.get<any[]>(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${periodId}/members`);
    }

    createGovernancePeriod(payload: any): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods`, payload);
    }

    updateGovernancePeriod(id: number, payload: any): Observable<any> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${id}`, payload);
    }

    activateGovernancePeriod(id: number): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${id}/activate`, {});
    }

    assignCommitteeRole(periodId: number, data: any): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${periodId}/members`, data);
    }

    removeCommitteeMember(ecMemberId: number, notifyMember: boolean): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/members/${ecMemberId}?notifyMember=${notifyMember}`);
    }

    // Audit log
    getGlobalActivityLog(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.ACTIVITY.ADMIN_GLOBAL);
    }

    // Error logs (45.4) — filtering/pagination is pushed to the DB query on the API side, not
    // an in-memory scan, since the table has no upper bound on row count.
    getErrorLogs(page: number = 1, pageSize: number = 20, level: string = 'all', query: string = '', fromDate?: string, toDate?: string): Observable<any> {
        let params = `?page=${page}&pageSize=${pageSize}`;
        if (level && level !== 'all') params += `&level=${encodeURIComponent(level)}`;
        if (query) params += `&query=${encodeURIComponent(query)}`;
        if (fromDate) params += `&fromDate=${encodeURIComponent(fromDate)}`;
        if (toDate) params += `&toDate=${encodeURIComponent(toDate)}`;
        return this.http.get<any>(`${API_ENDPOINTS.ADMIN.ERROR_LOGS}${params}`);
    }

    // System users and roles
    getSystemUsers(): Observable<any[]> {
        return this.http.get<any[]>(`${API_ENDPOINTS.ROLES}/users`);
    }

    getRoles(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.ROLES);
    }

    createSystemUser(data: any): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ROLES}/users`, data);
    }

    createCustomRole(roleName: string): Observable<any> {
        return this.http.post(API_ENDPOINTS.ROLES, JSON.stringify(roleName), {
            headers: { 'Content-Type': 'application/json' }
        });
    }

    assignUserRole(userId: number, roleName: string): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ROLES}/assign`, null, { params: { userId, roleName } });
    }

    removeUserRole(userId: number, roleName: string): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ROLES}/remove`, null, { params: { userId, roleName } });
    }

    resetSystemUserPassword(userId: number): Observable<any> {
        return this.http.post<any>(`${API_ENDPOINTS.ROLES}/users/${userId}/reset-password-admin`, {});
    }

    setSystemUserActive(userId: number, action: 'enable' | 'disable'): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.ROLES}/users/${userId}/${action}`, {});
    }

    deleteSystemUser(userId: number): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.ROLES}/users/${userId}`);
    }
}
