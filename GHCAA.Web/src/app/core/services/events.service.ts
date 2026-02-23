import { Injectable, signal } from '@angular/core';
import { of } from 'rxjs';

export interface AlumniEvent {
    id: number;
    title: string;
    description: string;
    date: Date;
    location: string;
    type: 'Reunion' | 'Seminar' | 'Networking' | 'Sports';
}

@Injectable({
    providedIn: 'root'
})
export class EventsService {
    private mockEvents: AlumniEvent[] = [
        { id: 1, title: 'Annual Grand Reunion 2025', description: 'Join us for the biggest gathering of Haragangians at the college campus.', date: new Date('2025-12-25T10:00:00'), location: 'College Playground', type: 'Reunion' },
        { id: 2, title: 'Corporate Career Path Seminar', description: 'Learning from the veterans on how to build a career in multinational companies.', date: new Date('2025-05-15T15:00:00'), location: 'Auditorium', type: 'Seminar' },
        { id: 3, title: 'Alumni Football Tournament', description: 'Inter-batch football tournament and friendly matches.', date: new Date('2025-08-10T09:00:00'), location: 'GHC Stadium', type: 'Sports' }
    ];

    getEvents() {
        return of(this.mockEvents);
    }
}
