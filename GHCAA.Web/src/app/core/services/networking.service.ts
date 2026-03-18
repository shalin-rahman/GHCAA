import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { MemberProfile, MemberSearchFilter } from '../models/business.models';

export interface PagedResult<T> {
    items: T[];
    totalItems: number;
    totalPages: number;
    page: number;
    pageSize: number;
    hasNextPage: boolean;
}

export interface MemberSummary {
    id: number;
    fullName: string;
    membershipNumber?: string;
    photoPath?: string;
    passingYear: number;
    ghcLastCertificate?: string;
    ghcLastCertificateGroup?: string;
    ghcLastCertificateSubject?: string;
    ghcLastCertificatePassingYear?: number;
    professionalSector?: string;
    designation?: string;
    bloodGroup?: string;
    email?: string;
    isEmailPublic?: boolean;
    mobileNo?: string;
    isMobilePublic?: boolean;
    membershipType?: string;
    category?: string;
    ecHistory: any[];
}

@Injectable({
    providedIn: 'root'
})
export class NetworkingService {
    private http = inject(HttpClient);

    getCommittee(params: any = {}, silent: boolean = false): Observable<MemberSummary[]> {
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<MemberSummary[]>(API_ENDPOINTS.NETWORKING.COMMITTEE, { params, headers });
    }

    getPeriods(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
    }

    searchMembers(filter: MemberSearchFilter & { page?: number; pageSize?: number }): Observable<PagedResult<MemberSummary>> {
        return this.http.get<PagedResult<MemberSummary>>(API_ENDPOINTS.NETWORKING.SEARCH, { params: filter as any });
    }

    getMemberProfile(id: number): Observable<any> {
        return this.http.get<any>(`${API_ENDPOINTS.NETWORKING.BASE}/member/${id}`);
    }

    getUpdates(params: any): Observable<MemberSummary[]> {
        return this.http.get<MemberSummary[]>(API_ENDPOINTS.NETWORKING.UPDATES, { params });
    }

    getRecentlyJoined(limit: number = 8): Observable<PagedResult<MemberSummary>> {
        return this.http.get<PagedResult<MemberSummary>>(API_ENDPOINTS.NETWORKING.SEARCH, { params: { pageSize: limit, sortBy: 'joinDate', sortDesc: true } });
    }

}
