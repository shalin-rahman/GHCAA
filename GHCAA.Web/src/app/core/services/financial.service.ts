import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface PaymentRecord {
    id: number;
    memberId: number;
    transactionId: string;
    amount: number;
    paidAt: string;
    status: any;
    category: any;
    notes?: string;
}

export interface MembershipDue {
    id: number;
    year: number;
    amount: number;
    isPaid: boolean;
    dueDate: string;
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
        return this.http.post(`${this.apiUrl}/record-payment`, dto);
    }
}
