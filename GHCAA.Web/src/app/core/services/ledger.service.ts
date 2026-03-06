import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface FinancialRecord {
    id: number;
    date: string;
    description: string;
    category: string;
    type: number; // 1 for Credit, 2 for Debit (Check Enums)
    amount: number;
    year: number;
}

export interface LedgerSummary {
    year: number;
    totalIncome: number;
    totalExpense: number;
    balance: number;
}

@Injectable({
    providedIn: 'root'
})
export class LedgerService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.LEDGER;

    getRecords(params: any): Observable<FinancialRecord[]> {
        return this.http.get<FinancialRecord[]>(this.apiUrl, { params });
    }

    getSummary(year: number): Observable<LedgerSummary> {
        return this.http.get<LedgerSummary>(`${this.apiUrl}/summary`, { params: { year: year.toString() } });
    }

    addRecord(record: Partial<FinancialRecord>): Observable<FinancialRecord> {
        return this.http.post<FinancialRecord>(this.apiUrl, record);
    }
}
