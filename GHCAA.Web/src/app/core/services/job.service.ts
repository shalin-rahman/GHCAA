import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { Job, CreateJobDto, UpdateJobDto } from '../models/business.models';


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

    createJob(job: CreateJobDto): Observable<Job> {
        return this.http.post<Job>(this.apiUrl, job);
    }


    updateJob(id: number, job: UpdateJobDto): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, job);
    }

}
