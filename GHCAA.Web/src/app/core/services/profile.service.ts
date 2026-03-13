import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AuthService } from './auth.service';

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
    private authService = inject(AuthService);
    private apiUrl = API_ENDPOINTS.PROFILE;

    getProfile(): Observable<MemberProfile> {
        const user = this.authService.currentUser();
        if (user && user.role === 'SuperAdmin') {
            return of({
                id: 0,
                fullName: 'System Administrator',
                email: 'superadmin@ghcaa.com',
                mobileNo: '00000000000',
                status: 1, // Active
                membershipType: 1, // Executive
                academicHistory: [],
                professionalHistory: [],
                presentAddress: 'Backend Server',
                permanentAddress: 'Backend Server',
                bloodGroup: 0,
                highestCertificate: 'Admin',
                highestCertificateGroup: 'Admin',
                highestCertificateSubject: 'Admin',
                highestCertificatePassingYear: 0,
                ghcLastCertificate: 'Admin',
                ghcLastCertificateGroup: 'Admin',
                ghcLastCertificateSubject: 'Admin',
                ghcLastCertificatePassingYear: 0,
                professionalSector: 'IT',
                designation: 'SuperAdmin',
                isMobilePublic: false,
                isEmailPublic: false,
                isAddressPublic: false
            } as MemberProfile);
        }
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
