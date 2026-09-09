import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { buildHttpParams } from '../utils/http.util';

export const MessageChannels = { Email: 'Email', Sms: 'Sms' } as const;
export type MessageChannel = typeof MessageChannels[keyof typeof MessageChannels];

export interface EmailTemplate {
    id: number;
    channel: MessageChannel;
    code: string;
    subject: string;
    body: string;
    description: string;
    variables?: string;
}

/** Canonical member + org-level variables the backend substitutes into template bodies (CommunicationService.BuildTemplateVariables). */
export const TEMPLATE_VARIABLES = {
    Member: [
        'FullName', 'FatherName', 'MotherName', 'DateOfBirth', 'Gender', 'BloodGroup', 'NID',
        'MembershipNumber', 'MembershipType', 'Email', 'MobileNo', 'PresentAddress', 'PermanentAddress',
        'Status', 'Category', 'AppliedDate', 'ApprovedDate',
        'PassingYear', 'HSCAdmissionYear', 'SubjectGroup', 'ProfessionalSector', 'Designation'
    ],
    Organization: ['OrgName', 'OrgShortName', 'SupportEmail', 'PortalUrl', 'CurrentYear']
};

export interface EmailLog {
    id: number;
    recipientEmail: string;
    subject: string;
    sentDate: string;
    status: string;
    templateCode?: string;
    targetAudience?: string;
    errorMessage?: string;
}

@Injectable({
    providedIn: 'root'
})
export class AdminCommService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.ADMIN.COMMUNICATION;

    getLogs(count: number = 100): Observable<EmailLog[]> {
        const params = buildHttpParams({ count });
        return this.http.get<EmailLog[]>(`${this.apiUrl}/logs`, { params });
    }

    getTemplates(): Observable<EmailTemplate[]> {
        return this.http.get<EmailTemplate[]>(`${this.apiUrl}/templates`);
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
        if (template.id === 0) {
            return this.http.post(`${this.apiUrl}/templates`, template);
        }
        return this.http.put(`${this.apiUrl}/templates/${template.id}`, template);
    }

    deleteTemplate(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/templates/${id}`);
    }
}
