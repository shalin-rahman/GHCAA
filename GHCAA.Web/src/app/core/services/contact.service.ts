import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface ContactMessage {
    fullName: string;
    email: string;
    subject: string;
    message: string;
}

@Injectable({
    providedIn: 'root'
})
export class ContactService {
    private http = inject(HttpClient);

    sendMessage(msg: any): Observable<any> {
        return this.http.post(API_ENDPOINTS.CONTACT, msg);
    }
}
