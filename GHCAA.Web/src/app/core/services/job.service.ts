import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
}

@Injectable({
    providedIn: 'root'
})
export class JobService {
    private http = inject(HttpClient);

    getJobs(category?: number): Observable<Job[]> {
        let params: Record<string, string> = {};
        if (category !== undefined) {
            params['category'] = category.toString();
        }
        return this.http.get<Job[]>('/api/jobs', { params });
    }

    getJobById(id: number): Observable<Job> {
        return this.http.get<Job>(`/api/jobs/${id}`);
    }

    postJob(job: Partial<Job>): Observable<Job> {
        return this.http.post<Job>('/api/jobs', job);
    }
}
