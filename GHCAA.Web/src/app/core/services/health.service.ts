import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({ providedIn: 'root' })
export class HealthService {
    private http = inject(HttpClient);

    check(): Observable<any> {
        return this.http.get<any>(API_ENDPOINTS.HEALTH);
    }
}
