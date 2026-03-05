import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmailTemplate {
    id: number;
    code: string;
    subject: string;
    body: string;
    description: string;
}

import { API_ENDPOINTS } from '../constants/api.endpoints';

@Injectable({
    providedIn: 'root'
})
export class AdminCommService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.ADMIN.COMMUNICATION;

    getLogs(): Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }

    getTemplates(): Observable<EmailTemplate[]> {
        return this.http.get<EmailTemplate[]>(`${this.apiUrl}/templates`);
    }

    sendMessage(payload: any): Observable<any> {
        return this.http.post(this.apiUrl, payload);
    }

    sendBatch(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/send-batch`, dto);
    }

    sendType(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/send-type`, dto);
    }

    sendCustom(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/send-custom`, dto);
    }

    saveTemplate(template: EmailTemplate): Observable<any> {
        return this.http.put(`${this.apiUrl}/templates/${template.id}`, template);
    }
}
