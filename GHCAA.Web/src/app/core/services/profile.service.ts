import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AuthService } from './auth.service';
import { 
    MembershipStatus, 
    MembershipType, 
    MemberCategory, 
    ECPosition, 
    Gender, 
    BloodGroup,
    AcademicRecord,
    ProfessionalRecord,
    MemberProfile,
    ECHistoryRecord
} from '../models/business.models';





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
                status: 'Active',
                membershipType: 'Executive',
                category: 'None',
                ecPosition: 'None',
                academicHistory: [],
                professionalHistory: [],
                presentAddress: 'Backend Server',
                permanentAddress: 'Backend Server',
                gender: 'Male',
                bloodGroup: 'APositive',
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
