import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

@Injectable({
    providedIn: 'root'
})
export class NetworkingService {
    private http = inject(HttpClient);

    getCommittee(params: any = {}): Observable<MemberProfile[]> {
        return this.http.get<MemberProfile[]>(API_ENDPOINTS.NETWORKING.COMMITTEE, { params });
    }


    getPeriods(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
    }

    searchMembers(filter: MemberSearchFilter & { page?: number; pageSize?: number }): Observable<PagedResult<MemberProfile>> {
        return this.http.get<PagedResult<MemberProfile>>(API_ENDPOINTS.NETWORKING.SEARCH, { params: filter as any });
    }



    getUpdates(params: any): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.UPDATES, { params });
    }

    getRecentlyJoined(limit: number = 8): Observable<PagedResult<MemberProfile>> {
        return this.http.get<PagedResult<MemberProfile>>(API_ENDPOINTS.NETWORKING.SEARCH, { params: { pageSize: limit, sortBy: 'joinDate', sortDesc: true } });
    }

}
