import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService, AlumniEvent, EventRegistration } from '../../core/services/events.service';
import { ExportButtonsComponent } from '../../shared/export-buttons/export-buttons.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { ExportUtil } from '../../core/utils/export.util';
import { NotificationService } from '../../core/services/notification.service';


@Component({
    selector: 'app-admin-events',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, ExportButtonsComponent, PaginationComponent],
    templateUrl: './admin-events.html',
    styleUrl: './admin-events.scss'
})
export class AdminEvents implements OnInit {
    private eventsService = inject(EventsService);
    private fb = inject(FormBuilder);
    private notify = inject(NotificationService);


    events = signal<AlumniEvent[]>([]);
    registrations = signal<any[]>([]);
    activeTab = signal<'manage' | 'approvals'>('manage');
    isExporting = signal(false);

    // Pagination & Filtering
    currentPage = signal(1);
    pageSize = signal(10);
    totalItems = signal(0);
    totalPages = signal(1);
    statusFilter = signal('all');
    searchQuery = signal('');
    selectedEventIdFilter = signal<number | null>(null);
    selectedEvent = signal<AlumniEvent | null>(null);

    // PDF Config
    pdfHeaders = ['ID', 'Event', 'Participant', 'Type', 'Reference', 'Status', 'Date'];
    pdfMapper = (r: any) => [
        r.id,
        r.eventTitle,
        r.memberName || r.guestName || 'Guest',
        r.isNonMember ? 'Guest' : 'Member',
        r.paymentReference,
        r.status,
        new Date(r.registeredAt).toLocaleDateString()
    ];

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
        isActive: [true],
        allowNonMembers: [false],
        imageUrl: ['']
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
        this.eventsService.getAllRegistrations(
            this.currentPage(), 
            this.pageSize(), 
            this.selectedEventIdFilter() || undefined, 
            this.statusFilter(), 
            this.searchQuery()
        ).subscribe({
            next: res => {
                this.registrations.set(res.items);
                this.totalItems.set(res.totalItems);
                this.totalPages.set(res.totalPages);
            },
            error: () => this.registrations.set([])
        });
    }

    onFilterChange() {
        this.currentPage.set(1);
        this.loadAllRegistrations();
    }

    handleExport(format: string) {
        this.isExporting.set(true);
        this.eventsService.getAllRegistrations(
            1, 10000, 
            this.selectedEventIdFilter() || undefined, 
            this.statusFilter(), 
            this.searchQuery()
        ).subscribe({
            next: (res: any) => {
                const data = res.items || [];
                if (format === 'excel') ExportUtil.toExcel(data, 'event_registrations');
                if (format === 'csv') ExportUtil.toCsv(data, 'event_registrations');
                if (format === 'pdf') {
                    const pData = data.map(this.pdfMapper);
                    ExportUtil.toPdf(this.pdfHeaders, pData, 'event_registrations', 'Event Registrations Export');
                }
                this.isExporting.set(false);
            },
            error: () => {
                this.isExporting.set(false);
            }
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
            isActive: ev.isActive,
            allowNonMembers: ev.allowNonMembers
        });
        this.showForm.set(true);
    }

    submitEvent() {
        if (this.eventForm.invalid) {
            this.notify.error('Please complete all required fields.');
            return;
        }

        this.isSubmitting.set(true);

        const raw = this.eventForm.value;
        
        // Helper to safely convert form dates to ISO strings for backend
        const toSafeISO = (val: any) => {
            if (!val) return undefined;
            const d = new Date(val);
            return isNaN(d.getTime()) ? undefined : d.toISOString();
        };

        const evData: Partial<AlumniEvent> = {
            title: raw.title || '',
            description: raw.description || '',
            location: raw.location || '',
            date: toSafeISO(raw.date)!,
            registrationDeadline: toSafeISO(raw.registrationDeadline),
            registrationFee: raw.registrationFee || 0,
            adminNote: raw.adminNote || undefined,
            isActive: raw.isActive ?? true,
            allowNonMembers: raw.allowNonMembers ?? false,
            imageUrl: raw.imageUrl || undefined
        };

        const id = this.editingEventId();

        const request = id
            ? this.eventsService.updateEvent(id, evData)
            : this.eventsService.createEvent(evData);

        request.subscribe({
            next: () => {
                this.notify.success(id ? 'Event configuration updated!' : 'New event launched successfully!');
                this.showForm.set(false);
                this.isSubmitting.set(false);
                this.loadAllEvents();
            },
            error: (err) => {
                console.error('Event operation failed:', err);
                this.notify.error(err.error?.message || 'Operation failed. Please ensure all fields are valid.');
                this.isSubmitting.set(false);
            }
        });
    }

    deleteEvent(id: number) {
        if (confirm('Are you sure you want to delete this event? This action cannot be undone.')) {
            this.eventsService.deleteEvent(id).subscribe({
                next: () => {
                    this.notify.success('Event removed from system');
                    this.loadAllEvents();
                },
                error: () => this.notify.error('Failed to delete event')

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

    viewEvent(ev: AlumniEvent) {
        this.selectedEvent.set(ev);
    }

    closeDetail() {
        this.selectedEvent.set(null);
    }
}
