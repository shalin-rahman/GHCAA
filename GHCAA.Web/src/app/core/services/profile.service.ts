import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface MemberProfile {
    id: number;
    fullName: string;
    email: string;
    mobileNo: string;
    membershipNumber?: string;
    status: number;
    membershipType: number;

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

    academicHistory: AcademicRecord[];
    professionalHistory: ProfessionalRecord[];
}

export interface AcademicRecord {
    id?: number;
    institutionName: string;
    degree: string;
    subject: string;
    admissionYear?: number;
    passingYear: number;
    isGHC: boolean;
    result?: string;
}

export interface ProfessionalRecord {
    id?: number;
    organizationName: string;
    designation: string;
    sector?: string;
    location?: string;
    startDate: string;
    endDate?: string;
    isCurrent: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ProfileService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.PROFILE;

    getProfile(): Observable<MemberProfile> {
        return this.http.get<MemberProfile>(this.apiUrl);
    }

    updateProfile(profile: any): Observable<any> {
        return this.http.put(this.apiUrl, profile);
    }

    getIDCard(): Observable<{ dataUri: string }> {
        return this.http.get<{ dataUri: string }>(`${this.apiUrl}/id-card`);
    }

    uploadPhoto(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('photo', file);
        return this.http.post<any>(`${this.apiUrl}/photo`, formData);
    }

    getCertificate(): Observable<{ dataUri: string }> {
        return this.http.get<{ dataUri: string }>(`${this.apiUrl}/certificate`);
    }
}
