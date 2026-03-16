import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PaymentConfig {
    id: number;
    method: string;
    displayName: string;
    description?: string;
    icon?: string;
    walletNumber?: string;
    accountHolderName?: string;
    bankName?: string;
    branchName?: string;
    accountNumber?: string;
    routingNumber?: string;
    instructions?: string;
    requiresReceipt: boolean;
    requiresReference: boolean;
    isEnabled: boolean;
    gateway?: string;
    gatewayPublicKey?: string;
    gatewaySecretKey?: string;
    gatewayCallbackUrl?: string;
    sortOrder: number;
    isOnline?: boolean;
}

@Injectable({ providedIn: 'root' })
export class PaymentConfigService {
    private http = inject(HttpClient);

    getActivePaymentMethods(): Observable<PaymentConfig[]> {
        return this.http.get<PaymentConfig[]>('/api/payment-config/active');
    }

    // Admin methods
    getAllConfigs(): Observable<any[]> {
        return this.http.get<any[]>('/api/payment-config/admin/all');
    }

    createConfig(config: any): Observable<any> {
        return this.http.post('/api/payment-config/admin', config);
    }

    updateConfig(id: number, config: any): Observable<any> {
        return this.http.put(`/api/payment-config/admin/${id}`, config);
    }

    toggleConfig(id: number): Observable<any> {
        return this.http.post(`/api/payment-config/admin/${id}/toggle`, {});
    }

    deleteConfig(id: number): Observable<any> {
        return this.http.delete(`/api/payment-config/admin/${id}`);
    }

    seedDefaults(): Observable<any> {
        return this.http.post('/api/payment-config/admin/seed-defaults', {});
    }
}
