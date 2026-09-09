import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { PaymentStatus, FinancialCategory, MembershipFeeConfig, CreateMembershipFeeConfig, UpdateMembershipFeeConfig } from '../models/business.models';
import { buildHttpParams } from '../utils/http.util';


/** Matches PaymentHistoryDto.cs */
export interface PaymentRecord {
    id: number;
    memberId: number;
    transactionId: string;
    amount: number;
    paidAt: string;
    status: PaymentStatus;
    financialCategory: FinancialCategory;
    paymentMethod: any; // Or proper enum if defined
    notes?: string;
}


/** Matches MembershipDueDto.cs */
export interface MembershipDue {
    id: number;
    memberId?: number;
    year: number;
    amount: number;
    isPaid: boolean;
    dueDate: string;
    paidAt?: string;
}

@Injectable({ providedIn: 'root' })
export class FinancialService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.FINANCIALS;

    getMyHistory(): Observable<PaymentRecord[]> {
        return this.http.get<PaymentRecord[]>(`${this.apiUrl}/my-history`);
    }

    getMyDues(): Observable<MembershipDue[]> {
        return this.http.get<MembershipDue[]>(`${this.apiUrl}/my-dues`);
    }

    recordPayment(dto: any): Observable<any> {
        // Correcting the endpoint use-case: if it's FormData, let HttpClient handle headers
        return this.http.post(`${this.apiUrl}/record-payment`, dto);
    }

    // Saved Payment Methods
    getSavedMethods(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/saved-methods`);
    }

    addSavedMethod(dto: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/saved-methods`, dto);
    }

    deleteSavedMethod(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/saved-methods/${id}`);
    }

    // 82.32: was `my-receipt/{id}`, which does not exist — the controller declares
    // `receipt/{paymentId}` (`FinancialsController.cs`) — so every click 404'd. Returned as an
    // Observable<Blob> rather than a plain URL string: the endpoint is behind [Authorize], and a
    // caller opening that URL directly in a new tab (`window.open`) sends no Authorization header,
    // so even the corrected path would 401. Fetching through HttpClient lets the auth interceptor
    // attach the token; the caller turns the blob into an object URL to display it.
    getReceipt(paymentId: number): Observable<Blob> {
        return this.http.get(`${this.apiUrl}/receipt/${paymentId}`, { responseType: 'blob' });
    }

    // Admin: Membership/Registration Fee Configs
    getFeeConfigs(): Observable<MembershipFeeConfig[]> {
        return this.http.get<MembershipFeeConfig[]>(`${this.apiUrl}/fees/config`);
    }

    addFeeConfig(dto: CreateMembershipFeeConfig): Observable<MembershipFeeConfig> {
        return this.http.post<MembershipFeeConfig>(`${this.apiUrl}/fees/config`, dto);
    }

    updateFeeConfig(dto: UpdateMembershipFeeConfig): Observable<MembershipFeeConfig> {
        return this.http.put<MembershipFeeConfig>(`${this.apiUrl}/fees/config`, dto);
    }

    getApplicableFee(category: string, type: string, date?: string): Observable<{ amount: number }> {
        const params = buildHttpParams({ category, type, date });
        return this.http.get<{ amount: number }>(`${this.apiUrl}/fees/applicable`, { params });
    }
}
