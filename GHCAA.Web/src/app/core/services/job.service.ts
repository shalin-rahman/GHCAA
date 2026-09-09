import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { Job, CreateJobDto, UpdateJobDto } from '../models/business.models';
import { buildHttpParams, getSilentHeaders } from '../utils/http.util';


@Injectable({
    providedIn: 'root'
})
export class JobService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.JOBS;

    getJobs(params?: any, silent: boolean = false): Observable<Job[]> {
        const headers = getSilentHeaders(silent);
        return this.http.get<Job[]>(this.apiUrl, { params, headers });
    }

    getJobById(id: number): Observable<Job> {
        return this.http.get<Job>(`${this.apiUrl}/${id}`);
    }

    createJob(job: CreateJobDto): Observable<Job> {
        return this.http.post<Job>(this.apiUrl, job);
    }


    updateJob(id: number, job: UpdateJobDto): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, job);
    }

    deleteJob(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    // Admin Approval
    getPendingJobs(): Observable<Job[]> {
        return this.http.get<Job[]>(`${this.apiUrl}/admin/pending`);
    }

    approveJob(id: number, notifyMember = true): Observable<any> {
        const params = buildHttpParams({ notifyMember });
        return this.http.post(`${this.apiUrl}/admin/${id}/approve`, {}, { params });
    }

    rejectJob(id: number, reason: string, notifyMember = true): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/${id}/reject`, { reason, notifyMember });
    }
}
