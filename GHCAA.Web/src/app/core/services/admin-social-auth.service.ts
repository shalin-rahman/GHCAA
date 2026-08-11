import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface SocialAuthConfig {
    id: number;
    provider: string;
    clientId: string;
    clientSecret?: string;
    isEnabled: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class AdminSocialAuthService {
    private http = inject(HttpClient);

    getConfigs(): Observable<SocialAuthConfig[]> {
        return this.http.get<SocialAuthConfig[]>(API_ENDPOINTS.ADMIN.SOCIAL_AUTH);
    }

    updateConfig(config: SocialAuthConfig): Observable<any> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.SOCIAL_AUTH}/${config.id}`, config);
    }
}
