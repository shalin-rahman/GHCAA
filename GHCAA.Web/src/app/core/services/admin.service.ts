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
    hscAdmissionYear: number;
    ghcAdmissionYear: number;
    lastCertificateFromGHC: string;
    subjectGroup: string;
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

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private http = inject(HttpClient);

    getPendingMembers(): Observable<MemberApprovalRequest[]> {
        // Note: The original backend used MemberService.GetAllMembersAsync. 
        // Usually admin/members.
        return this.http.get<MemberApprovalRequest[]>('/api/admin/members');
    }

    approveMember(id: number, adminId: number): Observable<any> {
        return this.http.post(`/api/admin/members/${id}/approve`, { approvedByAdminId: adminId });
    }

    rejectMember(id: number, adminId: number, reason: string): Observable<any> {
        return this.http.post(`/api/admin/members/${id}/reject`, { rejectedByAdminId: adminId, reason });
    }

    getStats(): Observable<any> {
        return this.http.get('/api/admin/stats');
    }

    archiveMember(id: number): Observable<any> {
        return this.http.delete(`/api/admin/members/${id}`);
    }

    reactivateMember(id: number): Observable<any> {
        return this.http.post(`/api/admin/members/${id}/reactivate`, {});
    }
}
