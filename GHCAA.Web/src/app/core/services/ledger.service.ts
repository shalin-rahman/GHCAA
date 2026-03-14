import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { FinancialRecord, LedgerSummary } from '../models/business.models';


@Injectable({
    providedIn: 'root'
})
export class LedgerService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.LEDGER;

    getRecords(params: any): Observable<any> {
        return this.http.get<any>(this.apiUrl, { params });
    }

    getSummary(year: number): Observable<LedgerSummary> {
        return this.http.get<LedgerSummary>(`${this.apiUrl}/summary`, { params: { year: year.toString() } });
    }

    addRecord(record: Partial<FinancialRecord>): Observable<FinancialRecord> {
        return this.http.post<FinancialRecord>(this.apiUrl, record);
    }
}
