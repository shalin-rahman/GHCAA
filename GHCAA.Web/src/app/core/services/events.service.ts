import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AlumniEvent, EventRegistration, EventParticipantSummary, EventTask, EventBudget, EventExpense, PagedRegistrations } from '../models/business.models';
import { buildHttpParams, getSilentHeaders } from '../utils/http.util';


@Injectable({
    providedIn: 'root'
})
export class EventsService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.EVENTS;

    getEvents(silent: boolean = false): Observable<AlumniEvent[]> {
        const headers = getSilentHeaders(silent);
        return this.http.get<AlumniEvent[]>(this.apiUrl, { headers });
    }


    getEventById(id: number): Observable<AlumniEvent> {
        return this.http.get<AlumniEvent>(`${this.apiUrl}/${id}`);
    }

    getPublicParticipants(eventId: number): Observable<EventParticipantSummary[]> {
        return this.http.get<EventParticipantSummary[]>(`${this.apiUrl}/${eventId}/participants`);
    }

    registerForEvent(dto: {
        eventId: number, 
        paymentReference: string, 
        paymentMethod?: string,
        isNonMember?: boolean,
        guestName?: string,
        guestEmail?: string,
        guestMobile?: string,
        contributionAmount?: number,
        receiptFile?: File
    }): Observable<any> {
        const formData = new FormData();
        formData.append('EventId', dto.eventId.toString());
        formData.append('PaymentReference', dto.paymentReference);
        
        if (dto.contributionAmount) {
            formData.append('ContributionAmount', dto.contributionAmount.toString());
        }
        
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
        return this.http.get<Array<EventRegistration & { event?: { title?: string } }>>(`${this.apiUrl}/my-registrations`).pipe(
            map(registrations => registrations.map(registration => ({
                ...registration,
                eventTitle: registration.eventTitle || registration.event?.title || (registration.eventId ? `Event #${registration.eventId}` : 'Untitled event')
            })))
        );
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
        const params = buildHttpParams({ page, pageSize, eventId, status, search });
        return this.http.get<any>(`${this.apiUrl}/admin/registrations`, { params });
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

    uploadEventLogo(id: number, file: File): Observable<any> {
        const formData = new FormData();
        formData.append('logo', file);
        return this.http.post(`${this.apiUrl}/admin/${id}/logo`, formData);
    }

    sendInvitationEmail(registrationId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/registrations/${registrationId}/send-invitation`, {});
    }

    // --- Operations (Tasks & Budget) ---

    getEventTasks(eventId: number): Observable<EventTask[]> {
        return this.http.get<EventTask[]>(`${this.apiUrl}/admin/${eventId}/tasks`);
    }

    createTask(dto: Partial<EventTask>): Observable<EventTask> {
        return this.http.post<EventTask>(`${this.apiUrl}/admin/tasks`, dto);
    }

    toggleTask(taskId: number): Observable<EventTask> {
        return this.http.post<EventTask>(`${this.apiUrl}/admin/tasks/${taskId}/toggle`, {});
    }

    deleteTask(taskId: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/admin/tasks/${taskId}`);
    }

    getEventBudget(eventId: number): Observable<EventBudget> {
        return this.http.get<EventBudget>(`${this.apiUrl}/admin/${eventId}/budget`);
    }

    updateBudget(dto: Partial<EventBudget>): Observable<EventBudget> {
        return this.http.post<EventBudget>(`${this.apiUrl}/admin/budget`, dto);
    }

    addExpense(dto: Partial<EventExpense>): Observable<EventExpense> {
        return this.http.post<EventExpense>(`${this.apiUrl}/admin/expenses`, dto);
    }

    deleteExpense(expenseId: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/admin/expenses/${expenseId}`);
    }
}
