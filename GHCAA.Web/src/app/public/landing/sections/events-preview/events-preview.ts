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

    ngOnInit() {
        this.eventsService.getEvents().subscribe({
            next: (data) => {
                const now = new Date();
                const active = data.filter(e => !e.registrationDeadline || new Date(e.registrationDeadline) >= now);
                const closed = data.filter(e => e.registrationDeadline && new Date(e.registrationDeadline) < now);
                
                // Get most recent closed event
                const latestClosed = closed.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 1);
                
                // Combine and sort by date
                const combined = [...active, ...latestClosed].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
                
                this.events.set(combined);
            },
            error: () => this.events.set([])
        });
    }

    isRegistrationClosed(deadline: string | Date | undefined): boolean {
        if (!deadline) return false;
        return new Date(deadline).getTime() < new Date().getTime();
    }
}


