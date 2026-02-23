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

@Injectable({ providedIn: 'root' })
export class AdminCommService {
    private http = inject(HttpClient);
    private apiUrl = '/api/admin/comm';

    getTemplates(): Observable<EmailTemplate[]> {
        return this.http.get<EmailTemplate[]>(`${this.apiUrl}/templates`);
    }

    updateTemplate(id: number, template: EmailTemplate): Observable<any> {
        return this.http.put(`${this.apiUrl}/templates/${id}`, template);
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
}
