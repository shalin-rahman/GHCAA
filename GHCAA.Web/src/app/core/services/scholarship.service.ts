import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    CreateScholarshipApplication, ScholarshipApplication, ScholarshipCall, ScholarshipFund, ScholarshipStatus
} from '../models/business.models';

@Injectable({ providedIn: 'root' })
export class ScholarshipService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.SCHOLARSHIPS;

    getPublicFunds(): Observable<ScholarshipFund[]> {
        return this.http.get<ScholarshipFund[]>(`${this.apiUrl}/public/funds`);
    }

    getPublicCalls(): Observable<ScholarshipCall[]> {
        return this.http.get<ScholarshipCall[]>(`${this.apiUrl}/public/calls`);
    }

    submitApplication(callId: number, dto: CreateScholarshipApplication): Observable<ScholarshipApplication> {
        return this.http.post<ScholarshipApplication>(`${this.apiUrl}/calls/${callId}/applications`, dto);
    }

    getStatus(referenceCode: string, email: string): Observable<ScholarshipStatus> {
        const params = new HttpParams().set('email', email);
        return this.http.get<ScholarshipStatus>(`${this.apiUrl}/status/${encodeURIComponent(referenceCode)}`, { params });
    }
}
