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

    getPendingMembers(): Observable<MemberApprovalRequest[]> {
        return this.http.get<MemberApprovalRequest[]>(this.apiUrl);
    }

    getMembers(includeArchived: boolean = false): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}?includeArchived=${includeArchived}`);
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

    sendPasswordResetLink(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reset-password-admin`, {});
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
