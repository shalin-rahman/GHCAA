import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventsService, AlumniEvent } from '../../core/services/events.service';

@Component({
    selector: 'app-events',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="events-page">
      <div class="header">
        <h2>Upcoming Events</h2>
        <p>Save the date for our upcoming gatherings and professional sessions</p>
      </div>

      <div class="events-list">
        @for (ev of events(); track ev.id) {
          <div class="event-card glass-card">
            <div class="date-badge">
                <span class="day">{{ ev.date | date:'dd' }}</span>
                <span class="month">{{ ev.date | date:'MMM' }}</span>
            </div>
            <div class="event-details">
                <div class="type">{{ ev.type }}</div>
                <h3>{{ ev.title }}</h3>
                <p>{{ ev.description }}</p>
                <div class="footer">
                    <span>📍 {{ ev.location }}</span>
                    <span>⏰ {{ ev.date | date:'shortTime' }}</span>
                </div>
            </div>
            <div class="actions">
                <button class="btn btn-primary">RSVP Now</button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
    styles: [`
    .events-page { padding-bottom: 5rem; }
    .header { margin-bottom: 3.5rem; }
    
    .events-list { display: flex; flex-direction: column; gap: 2rem; max-width: 900px; }
    
    .event-card { display: flex; gap: 2rem; padding: 2rem; align-items: center; }
    
    .date-badge { width: 80px; height: 80px; background: var(--primary-color); color: var(--accent-color); border-radius: 16px; display: flex; flex-direction: column; align-items: center; justify-content: center; flex-shrink: 0; }
    .day { font-size: 1.75rem; font-weight: 800; line-height: 1; }
    .month { font-size: 0.8rem; font-weight: 800; text-transform: uppercase; margin-top: 4px; }
    
    .event-details { flex: 1; display: flex; flex-direction: column; gap: 0.5rem; }
    .type { font-size: 0.7rem; font-weight: 800; text-transform: uppercase; color: var(--accent-color); letter-spacing: 1px; }
    .event-details h3 { font-size: 1.4rem; color: var(--text-main); }
    .event-details p { font-size: 0.95rem; color: var(--text-muted); line-height: 1.6; }
    
    .footer { display: flex; gap: 2rem; margin-top: 0.75rem; font-size: 0.85rem; font-weight: 700; color: var(--text-muted); }
    
    .actions { padding-left: 2rem; border-left: 1px solid var(--glass-border); flex-shrink: 0; }

    @media (max-width: 768px) {
        .event-card { flex-direction: column; align-items: flex-start; gap: 1.5rem; }
        .actions { border: none; padding: 0; width: 100%; }
        .actions button { width: 100%; }
    }
  `]
})
export class Events implements OnInit {
    private eventsService = inject(EventsService);
    events = signal<AlumniEvent[]>([]);

    ngOnInit() {
        this.eventsService.getEvents().subscribe(data => this.events.set(data));
    }
}
