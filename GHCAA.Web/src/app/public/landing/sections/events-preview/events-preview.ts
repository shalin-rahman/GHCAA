import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EventsService } from '../../../../core/services/events.service';
import { AlumniEvent } from '../../../../core/models/business.models';

@Component({
    selector: 'landing-events',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './events-preview.html',
    styleUrl: './events-preview.scss',
})
export class LandingEventsPreview implements OnInit {
    private eventsService = inject(EventsService);
    events = signal<AlumniEvent[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        this.eventsService.getEvents(true).subscribe({
            next: (data) => {
                const now = new Date();
                const active = data.filter(e => !e.registrationEndDate || new Date(e.registrationEndDate) >= now);
                const closed = data.filter(e => e.registrationEndDate && new Date(e.registrationEndDate) < now);

                // Get most recent closed event
                const latestClosed = closed.sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime()).slice(0, 1);

                // Combine and sort by startDate
                const combined = [...active, ...latestClosed].sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime());

                this.events.set(combined);
            },
            error: () => {
                this.events.set([]);
                this.isVisible.set(false);
            }
        });
    }

    isRegistrationClosed(ev: AlumniEvent): boolean {
        if (!ev.registrationEndDate) return false;
        return new Date(ev.registrationEndDate).getTime() < new Date().getTime();
    }
}


