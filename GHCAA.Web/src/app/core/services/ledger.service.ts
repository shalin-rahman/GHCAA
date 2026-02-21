import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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

    getRecords(year?: number, type?: number): Observable<FinancialRecord[]> {
        let params: any = {};
        if (year) params.year = year.toString();
        if (type) params.type = type.toString();
        return this.http.get<FinancialRecord[]>('/api/ledger', { params });
    }

    getSummary(year: number): Observable<LedgerSummary> {
        return this.http.get<LedgerSummary>('/api/ledger/summary', { params: { year: year.toString() } });
    }

    addRecord(record: Partial<FinancialRecord>): Observable<FinancialRecord> {
        return this.http.post<FinancialRecord>('/api/ledger', record);
    }
}
