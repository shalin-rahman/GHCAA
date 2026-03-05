import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EventsService, AlumniEvent } from '../../../../core/services/events.service';

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
            next: (data) => this.events.set(data.slice(0, 3)),
            error: () => this.events.set([])
        });
    }
}
