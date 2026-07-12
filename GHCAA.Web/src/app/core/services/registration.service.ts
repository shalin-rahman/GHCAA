import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({
    providedIn: 'root'
})
export class RegistrationService {
    private http = inject(HttpClient);

    register(formData: FormData): Observable<any> {
        return this.http.post(API_ENDPOINTS.AUTH.REGISTER, formData);
    }

    verifyEmail(email: string, otpCode: string): Observable<any> {
        return this.http.post(API_ENDPOINTS.AUTH.VERIFY_EMAIL, { email, otpCode });
    }

    resendOtp(email: string): Observable<any> {
        return this.http.post(API_ENDPOINTS.AUTH.RESEND_OTP, { email });
    }

    getStatus(id: number, email: string): Observable<any> {
        return this.http.get(`${API_ENDPOINTS.AUTH.STATUS}/${id}`, { params: { email } });
    }

    getPublicPaymentConfigs(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.PAYMENT_CONFIG.PUBLIC);
    }
}
