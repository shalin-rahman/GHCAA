import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api.endpoints';

export interface AlumniEvent {
    id: number;
    title: string;
    description: string;
    date: string;
    location: string;
    registrationFee?: number;
    type?: string;
    isActive: boolean;
    imageUrl?: string;
    registrationDeadline?: string;
    adminNote?: string;
}

export interface EventRegistration {
    id: number;
    eventId: number;
    event?: AlumniEvent;
    memberId: number;
    memberName?: string;
    paymentReference: string;
    receiptPath?: string;
    status: 'Pending' | 'Approved' | 'Rejected';
    registeredAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class EventsService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.EVENTS;

    getEvents(): Observable<AlumniEvent[]> {
        return this.http.get<AlumniEvent[]>(this.apiUrl);
    }

    getEventById(id: number): Observable<AlumniEvent> {
        return this.http.get<AlumniEvent>(`${this.apiUrl}/${id}`);
    }

    registerForEvent(eventId: number, paymentRef: string, receiptFile?: File): Observable<any> {
        const formData = new FormData();
        formData.append('EventId', eventId.toString());
        formData.append('PaymentReference', paymentRef);
        if (receiptFile) {
            formData.append('receipt', receiptFile);
        }
        return this.http.post(`${this.apiUrl}/register`, formData);
    }

    getMyRegistrations(): Observable<EventRegistration[]> {
        return this.http.get<EventRegistration[]>(`${this.apiUrl}/my-registrations`);
    }

    // Admin Methods
    getAllEventsForAdmin(): Observable<AlumniEvent[]> {
        return this.http.get<AlumniEvent[]>(`${this.apiUrl}/admin/all`);
    }

    createEvent(ev: Partial<AlumniEvent>): Observable<AlumniEvent> {
        return this.http.post<AlumniEvent>(`${this.apiUrl}/admin`, ev);
    }

    getAllRegistrations(): Observable<EventRegistration[]> {
        return this.http.get<EventRegistration[]>(`${this.apiUrl}/admin/registrations`);
    }

    approveRegistration(registrationId: number, approve: boolean): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/approve-registration`, { registrationId, approve });
    }
}
