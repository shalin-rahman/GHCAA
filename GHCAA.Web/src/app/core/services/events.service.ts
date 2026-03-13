import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface AlumniEvent {
    id: number;
    title: string;
    description: string;
    date: string;
    location: string;
    registrationFee?: number;
    type?: string;
    isActive: boolean;
    allowNonMembers: boolean;
    imageUrl?: string;
    registrationDeadline?: string;
    adminNote?: string;
}

export interface EventRegistration {
    id: number;
    eventId: number;
    event?: AlumniEvent;
    memberId?: number;
    memberName?: string;
    isNonMember: boolean;
    guestName?: string;
    guestEmail?: string;
    guestMobile?: string;
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

    registerForEvent(dto: {
        eventId: number, 
        paymentReference: string, 
        paymentMethod?: string,
        isNonMember?: boolean,
        guestName?: string,
        guestEmail?: string,
        guestMobile?: string,
        receiptFile?: File
    }): Observable<any> {
        const formData = new FormData();
        formData.append('EventId', dto.eventId.toString());
        formData.append('PaymentReference', dto.paymentReference);
        
        if (dto.paymentMethod) {
            formData.append('PaymentMethod', dto.paymentMethod);
        }
        
        if (dto.isNonMember) {
            formData.append('IsNonMember', 'true');
            if (dto.guestName) formData.append('GuestName', dto.guestName);
            if (dto.guestEmail) formData.append('GuestEmail', dto.guestEmail);
            if (dto.guestMobile) formData.append('GuestMobile', dto.guestMobile);
        }

        if (dto.receiptFile) {
            formData.append('receipt', dto.receiptFile);
        }
        return this.http.post(`${this.apiUrl}/register`, formData);
    }

    getMyRegistrations(): Observable<EventRegistration[]> {
        return this.http.get<EventRegistration[]>(`${this.apiUrl}/my-registrations`);
    }

    getRegistrationForInvitation(id: number): Observable<EventRegistration> {
        return this.http.get<EventRegistration>(`${this.apiUrl}/registration/${id}`);
    }

    // Admin Methods
    getAllEventsForAdmin(): Observable<AlumniEvent[]> {
        return this.http.get<AlumniEvent[]>(`${this.apiUrl}/admin/all`);
    }

    createEvent(ev: Partial<AlumniEvent>): Observable<AlumniEvent> {
        return this.http.post<AlumniEvent>(`${this.apiUrl}/admin`, ev);
    }

    getAllRegistrations(page: number = 1, pageSize: number = 10, eventId?: number, status?: string, search?: string): Observable<any> {
        let url = `${this.apiUrl}/admin/registrations?page=${page}&pageSize=${pageSize}`;
        if (eventId) url += `&eventId=${eventId}`;
        if (status) url += `&status=${status}`;
        if (search) url += `&search=${search}`;
        return this.http.get<any>(url);
    }

    approveRegistration(registrationId: number, approve: boolean): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/approve-registration`, { registrationId, approve });
    }

    updateEvent(id: number, ev: Partial<AlumniEvent>): Observable<AlumniEvent> {
        return this.http.put<AlumniEvent>(`${this.apiUrl}/admin/${id}`, ev);
    }

    deleteEvent(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/admin/${id}`);
    }
}
