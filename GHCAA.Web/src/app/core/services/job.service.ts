import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface Job {
    id: number;
    title: string;
    companyName: string;
    location: string;
    category: number;
    description: string;
    requirements: string;
    benefits?: string;
    salaryRange?: string;
    deadline: string;
    postedByMemberId: number;
    postedByMemberName: string;
    isClosed: boolean;
    createdAt: string;
    applicationEmail?: string;
}

@Injectable({
    providedIn: 'root'
})
export class JobService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.JOBS;

    getJobs(params?: any): Observable<Job[]> {
        return this.http.get<Job[]>(this.apiUrl, { params });
    }

    getJobById(id: number): Observable<Job> {
        return this.http.get<Job>(`${this.apiUrl}/${id}`);
    }

    createJob(job: Partial<Job>): Observable<Job> {
        return this.http.post<Job>(this.apiUrl, job);
    }

    updateJob(id: number, job: Partial<Job>): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, job);
    }
}
