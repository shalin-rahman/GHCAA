import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService, AlumniEvent, EventRegistration } from '../../core/services/events.service';

@Component({
    selector: 'app-admin-events',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './admin-events.html',
    styleUrl: './admin-events.scss'
})
export class AdminEvents implements OnInit {
    private eventsService = inject(EventsService);
    private fb = inject(FormBuilder);

    events = signal<AlumniEvent[]>([]);
    registrations = signal<EventRegistration[]>([]);
    activeTab = signal<'manage' | 'approvals'>('manage');

    // Form handling
    showForm = signal<boolean>(false);
    editingEventId = signal<number | null>(null);
    isSubmitting = signal<boolean>(false);

    eventForm = this.fb.group({
        title: ['', Validators.required],
        description: ['', Validators.required],
        date: ['', Validators.required],
        location: ['', Validators.required],
        registrationFee: [0],
        registrationDeadline: [''],
        adminNote: [''],
        isActive: [true]
    });

    ngOnInit() {
        this.loadAllEvents();
        this.loadAllRegistrations();
    }

    loadAllEvents() {
        this.eventsService.getAllEventsForAdmin().subscribe({
            next: data => this.events.set(data),
            error: () => this.events.set([])
        });
    }

    loadAllRegistrations() {
        this.eventsService.getAllRegistrations().subscribe({
            next: data => this.registrations.set(data),
            error: () => this.registrations.set([])
        });
    }

    openCreateForm() {
        this.editingEventId.set(null);
        this.eventForm.reset({ isActive: true, registrationFee: 0 });
        this.showForm.set(true);
    }

    openEditForm(ev: AlumniEvent) {
        this.editingEventId.set(ev.id);
        this.eventForm.patchValue({
            title: ev.title,
            description: ev.description,
            date: ev.date ? new Date(ev.date).toISOString().slice(0, 16) : '',
            location: ev.location,
            registrationFee: ev.registrationFee,
            registrationDeadline: ev.registrationDeadline ? new Date(ev.registrationDeadline).toISOString().slice(0, 16) : '',
            adminNote: ev.adminNote,
            isActive: ev.isActive
        });
        this.showForm.set(true);
    }

    submitEvent() {
        if (this.eventForm.invalid) return;
        this.isSubmitting.set(true);

        const evData = this.eventForm.value as Partial<AlumniEvent>;
        const id = this.editingEventId();

        const request = id
            ? this.eventsService.updateEvent(id, evData)
            : this.eventsService.createEvent(evData);

        request.subscribe({
            next: () => {
                alert(id ? 'Event updated successfully!' : 'Event created successfully!');
                this.showForm.set(false);
                this.isSubmitting.set(false);
                this.loadAllEvents();
            },
            error: (err) => {
                console.error(err);
                alert('Operation failed.');
                this.isSubmitting.set(false);
            }
        });
    }

    deleteEvent(id: number) {
        if (confirm('Are you sure you want to delete this event? This action cannot be undone.')) {
            this.eventsService.deleteEvent(id).subscribe({
                next: () => {
                    alert('Event deleted.');
                    this.loadAllEvents();
                },
                error: () => alert('Failed to delete event.')
            });
        }
    }

    approveReg(regId: number, approve: boolean) {
        const action = approve ? 'approve' : 'reject';
        if (confirm(`Are you sure you want to ${action} this registration?`)) {
            this.eventsService.approveRegistration(regId, approve).subscribe(() => {
                this.loadAllRegistrations();
            });
        }
    }
}
