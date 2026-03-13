import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MemberApprovalRequest {
    id: number;
    fullName: string;
    fatherName: string;
    motherName: string;
    dateOfBirth: string;
    gender: string;
    bloodGroup: string;
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
    membershipType: number;
    status: number;
    photoPath?: string;
    certificatePath?: string;
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

    getMembers(page: number = 1, pageSize: number = 10, searchQuery: string = '', statusFilter: string = 'all', includeArchived: boolean = false): Observable<any> {
        let params = `?page=${page}&pageSize=${pageSize}&includeArchived=${includeArchived}&statusFilter=${statusFilter}`;
        if (searchQuery) params += `&searchQuery=${encodeURIComponent(searchQuery)}`;
        return this.http.get<any>(`${this.apiUrl}${params}`);
    }

    getMemberById(id: number): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/${id}`);
    }

    getAllForExport(searchQuery: string = '', statusFilter: string = 'all'): Observable<any[]> {
        // Fetch a large number or use a specific export endpoint if you add one later
        let params = `?page=1&pageSize=10000&statusFilter=${statusFilter}`;
        if (searchQuery) params += `&searchQuery=${encodeURIComponent(searchQuery)}`;
        return this.http.get<any>(`${this.apiUrl}${params}`);
    }

    approveMember(id: number, adminId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/approve`, { approvedByAdminId: adminId });
    }

    rejectMember(id: number, adminId: number, reason: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reject`, { rejectedByAdminId: adminId, reason });
    }

    getStats(): Observable<any> {
        return this.http.get(API_ENDPOINTS.ADMIN.STATS);
    }

    archiveMember(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
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
}
