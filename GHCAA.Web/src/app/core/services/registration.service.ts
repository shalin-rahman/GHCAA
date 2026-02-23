import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class RegistrationService {
    private http = inject(HttpClient);

    register(formData: FormData): Observable<any> {
        return this.http.post('/api/auth/register', formData);
    }

    verifyEmail(email: string, otpCode: string): Observable<any> {
        return this.http.post('/api/auth/verify-email', { email, otpCode });
    }

    getStatus(id: number): Observable<any> {
        return this.http.get(`/api/auth/status/${id}`);
    }
}
