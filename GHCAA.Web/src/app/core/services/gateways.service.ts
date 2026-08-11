import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { PaymentGateway } from '../models/business.models';

export interface InitiatePaymentRequest {
    amount: number;
    gateway: PaymentGateway;
    reference: string;
    baseUrl: string;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
}

export interface InitiatePaymentResponse {
    success: boolean;
    message: string;
    gatewayUrl?: string;
}

@Injectable({
    providedIn: 'root'
})
export class GatewaysService {
    private http = inject(HttpClient);
    private apiUrl = '/api/gateways';

    initiatePayment(request: InitiatePaymentRequest): Observable<InitiatePaymentResponse> {
        return this.http.post<InitiatePaymentResponse>(`${this.apiUrl}/initiate`, request);
    }
}
