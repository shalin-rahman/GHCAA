import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export enum PaymentGateway {
    SSLCommerz = 0,
    Bkash = 1
}

export interface InitiatePaymentRequest {
    amount: number;
    gateway: PaymentGateway;
    reference: string;
    baseUrl: string;
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
