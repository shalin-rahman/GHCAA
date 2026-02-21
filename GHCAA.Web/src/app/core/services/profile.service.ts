import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MemberProfile {
    id: number;
    fullName: string;
    email: string;
    mobileNo: string;
    membershipNumber?: string;
    status: number;
    membershipType: number;

    // Academic
    ghcLastCertificatePassingYear: number;
    lastCertificateFromGHC: string;
    subjectGroup: string;

    // Professional
    professionalSector: string;
    designation: string;

    // Info
    photoPath?: string;
    presentAddress: string;
    permanentAddress: string;
    bloodGroup: number;

    // Privacy
    isMobilePublic: boolean;
    isEmailPublic: boolean;
    isAddressPublic: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ProfileService {
    private http = inject(HttpClient);

    getProfile(): Observable<MemberProfile> {
        return this.http.get<MemberProfile>('/api/profile');
    }

    updateProfile(profile: Partial<MemberProfile>): Observable<any> {
        return this.http.put('/api/profile', profile);
    }

    getIDCard(): Observable<{ dataUri: string }> {
        return this.http.get<{ dataUri: string }>('/api/profile/id-card');
    }

    getCertificate(): Observable<{ dataUri: string }> {
        return this.http.get<{ dataUri: string }>('/api/profile/certificate');
    }
}
