import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { MemberProfile, MemberSearchFilter, MembershipStatus, MemberCategory, BloodGroup, MembershipType } from '../models/business.models';
import { getSilentHeaders } from '../utils/http.util';

export interface PagedResult<T> {
    items: T[];
    totalItems: number;
    totalPages: number;
    page: number;
    pageSize: number;
}

export interface MemberSummary {
    id: number;
    memberId: number;
    fullName: string;
    fatherName?: string;
    motherName?: string;
    email: string;
    mobileNo: string;
    hscBatch?: string;
    passingYear?: number;
    hscAdmissionYear?: number;
    bloodGroup?: BloodGroup;
    category?: MemberCategory;
    membershipType?: MembershipType;
    membershipNumber?: string;
    designation?: string;
    companyName?: string;
    professionalSector?: string;
    photoUrl?: string;
    status: MembershipStatus;
    appliedDate: string;
    approvedDate?: string;
    committeeRole?: string;
    ecPeriodName?: string;
}

@Injectable({
    providedIn: 'root'
})
export class NetworkingService {
    private http = inject(HttpClient);

    getCommittee(params: any = {}, silent: boolean = false): Observable<MemberSummary[]> {
        const headers = getSilentHeaders(silent);
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
