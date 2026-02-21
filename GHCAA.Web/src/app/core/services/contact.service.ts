import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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

    submitMessage(msg: ContactMessage): Observable<any> {
        return this.http.post('/api/contact', msg);
    }
}
