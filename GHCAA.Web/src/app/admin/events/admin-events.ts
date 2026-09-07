import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent, EventRegistration } from '../../core/models/business.models';
import { ExportButtonsComponent } from '../../common/export-buttons/export-buttons.component';
import { PaginationComponent } from '../../common/pagination/pagination.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ExportUtil } from '../../core/utils/export.util';
import { validateUploadFile } from '../../core/utils/file-validation.util';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NavService } from '../../core/services/nav.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { getEventStatusMeta } from '../../core/utils/date.util';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

// 82.32: `new Date(x).toISOString().slice(0, 16)` formats in UTC, but `<input type="datetime-local">`
// always reads/writes local wall-clock time. Loading an event's UTC-formatted date into that input
// displayed the wrong time, and saving it again ran another local-to-UTC conversion on top of the
// first, shifting the stored date by the timezone offset on every edit. This formats from the local
// Date parts instead, matching what the input actually expects.
function toLocalDateTimeInputValue(value: string | Date): string {
    const d = new Date(value);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}


@Component({
    selector: 'app-admin-events',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, ExportButtonsComponent, PaginationComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective, LogoSpinnerComponent, ModalHeaderComponent],
    templateUrl: './admin-events.html',
    styleUrl: './admin-events.scss'
})
export class AdminEvents implements OnInit {
    private eventsService = inject(EventsService);
    private fb = inject(FormBuilder);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    public nav = inject(NavService);
    public orgConfigService = inject(OrgConfigService);


    // Date-computed lifecycle badge (Upcoming/Ongoing/Ended/Unpublished) — replaces the raw
    // isActive-only Active/Archived badge, which never reflected the event's own dates.
    getStatusMeta = getEventStatusMeta;

    events = signal<AlumniEvent[]>([]);
    registrations = signal<any[]>([]);
    loading = signal<boolean>(true);
    loadingRegistrations = signal<boolean>(true);
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
    showInvitation = signal<boolean>(false);
    invitationData = signal<any | null>(null);
    formError = signal<string | null>(null);

    filteredEvents = computed(() => {
        const query = this.searchQuery().toLowerCase();
        const all = this.events();
        if (!query) return all;
        return all.filter(e => 
            e.title?.toLowerCase()?.includes(query) || 
            e.location?.toLowerCase()?.includes(query)
        );
    });
    
    // Receipt Preview
    showReceiptModal = signal<boolean>(false);
    activeReceiptUrl = signal<string | null>(null);

    // PDF Config
    pdfHeaders = ['ID', 'Event', 'Participant', 'Type', 'Amount', 'Reference', 'Status', 'Date'];
    pdfMapper = (r: any) => [
        r.id,
        r.eventTitle,
        r.memberName || r.guestName || 'Guest',
        r.isNonMember ? 'Guest' : 'Member',
        (r.contributionAmount || r.eventFee || 0).toFixed(2),
        r.paymentReference,
        r.status,
        new Date(r.registeredAt).toLocaleDateString()
    ];

    // Form handling
    showForm = signal<boolean>(false);
    editingEventId = signal<number | null>(null);
    isSubmitting = signal<boolean>(false);
    selectedLogo = signal<File | null>(null);
    logoPreview = signal<string | null>(null);

    /** Cross-field date validation: endDate > startDate; regEnd > regStart; regEnd ≤ startDate */
    static eventDatesValidator(group: AbstractControl): ValidationErrors | null {
        const start = group.get('startDate')?.value;
        const end = group.get('endDate')?.value;
        const regStart = group.get('registrationStartDate')?.value;
        const regEnd = group.get('registrationEndDate')?.value;

        const errors: ValidationErrors = {};

        if (start && end && new Date(end) <= new Date(start)) {
            errors['endBeforeStart'] = 'Event end date must be after the start date.';
        }
        if (regStart && regEnd && new Date(regEnd) <= new Date(regStart)) {
            errors['regEndBeforeRegStart'] = 'Registration close date must be after the registration open date.';
        }
        if (regEnd && start && new Date(regEnd) > new Date(start)) {
            errors['regEndAfterEventStart'] = 'Registration should close on or before the event start date.';
        }

        return Object.keys(errors).length ? errors : null;
    }

    eventForm = this.fb.group({
        title: ['', Validators.required],
        description: ['', Validators.required],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required],
        location: ['', Validators.required],
        registrationFee: [0],
        requiresPayment: [true],
        registrationStartDate: [''],
        registrationEndDate: [''],
        adminNote: [''],
        isActive: [true],
        allowNonMembers: [false],
        requiresRegistration: [true],
        imageUrl: [''],
        // 82.32: explicitly typed — Angular's typed reactive forms infer a control's type from its
        // initial value alone, so a bare `[null]` produced FormControl<null>, which could never
        // legally hold the number patchValue assigns to it once AlumniEvent.participantLimit was
        // typed as `number | null`.
        participantLimit: [null as number | null],
        hasWaitlist: [false],
        // 82.52: create defaults to notifying (matches what this already did unconditionally
        // when published); edit defaults to not re-notifying (this never notified before).
        notifyMembers: [true]
    }, { validators: AdminEvents.eventDatesValidator });

    ngOnInit() {
        this.loadAllEvents();
        this.loadAllRegistrations();
    }

    loadAllEvents() {
        this.loading.set(true);
        this.eventsService.getAllEventsForAdmin().subscribe({
            next: data => { this.events.set(data); this.loading.set(false); },
            error: () => { this.events.set([]); this.loading.set(false); }
        });
    }

    loadAllRegistrations() {
        this.loadingRegistrations.set(true);
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
                this.loadingRegistrations.set(false);
            },
            error: () => { this.registrations.set([]); this.loadingRegistrations.set(false); }
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
        this.eventForm.reset({ isActive: true, requiresPayment: true, requiresRegistration: true, registrationFee: 0, notifyMembers: true });
        this.selectedLogo.set(null);
        this.logoPreview.set(null);
        this.formError.set(null);
        this.showForm.set(true);
    }

    openEditForm(ev: AlumniEvent) {
        this.editingEventId.set(ev.id);
        this.eventForm.patchValue({
            title: ev.title,
            description: ev.description,
            startDate: ev.startDate ? toLocalDateTimeInputValue(ev.startDate) : '',
            endDate: ev.endDate ? toLocalDateTimeInputValue(ev.endDate) : '',
            location: ev.location,
            registrationFee: ev.registrationFee,
            requiresPayment: ev.requiresPayment,
            registrationStartDate: ev.registrationStartDate ? toLocalDateTimeInputValue(ev.registrationStartDate) : '',
            registrationEndDate: ev.registrationEndDate ? toLocalDateTimeInputValue(ev.registrationEndDate) : '',
            adminNote: ev.adminNote,
            isActive: ev.isActive,
            allowNonMembers: ev.allowNonMembers,
            requiresRegistration: ev.requiresRegistration ?? true,
            participantLimit: ev.participantLimit,
            hasWaitlist: ev.hasWaitlist,
            notifyMembers: false
        });
        this.selectedLogo.set(null);
        this.logoPreview.set(ev.imageUrl || null);
        this.formError.set(null);
        this.showForm.set(true);
    }

    onLogoSelected(event: any) {
        const file: File = event.target.files[0];
        if (!file) return;
        const err = validateUploadFile(file, 'image');
        if (err) { this.notify.error(err); event.target.value = ''; return; }
        this.selectedLogo.set(file);
        const reader = new FileReader();
        reader.onload = () => this.logoPreview.set(reader.result as string);
        reader.readAsDataURL(file);
    }

    submitEvent() {
        this.eventForm.markAllAsTouched();

        if (this.eventForm.invalid) {
            const errors = this.eventForm.errors;
            if (errors?.['endBeforeStart']) {
                this.notify.error(errors['endBeforeStart']);
            } else if (errors?.['regEndBeforeRegStart']) {
                this.notify.error(errors['regEndBeforeRegStart']);
            } else if (errors?.['regEndAfterEventStart']) {
                this.notify.error(errors['regEndAfterEventStart']);
            } else {
                this.notify.error('Please complete all required fields.');
            }
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
            startDate: toSafeISO(raw.startDate)!,
            endDate: toSafeISO(raw.endDate)!,
            registrationStartDate: toSafeISO(raw.registrationStartDate),
            registrationEndDate: toSafeISO(raw.registrationEndDate),
            registrationFee: raw.registrationFee || 0,
            requiresPayment: raw.requiresPayment ?? true,
            adminNote: raw.adminNote || undefined,
            isActive: raw.isActive ?? true,
            allowNonMembers: raw.allowNonMembers ?? false,
            requiresRegistration: raw.requiresRegistration ?? true,
            imageUrl: raw.imageUrl || undefined,
            // 82.32: the form has these fields and the API accepts them (CreateEventDto/
            // UpdateEventDto), but the save payload never included them, so every save silently
            // wiped whatever limit/waitlist setting an admin had entered.
            participantLimit: raw.participantLimit ?? null,
            hasWaitlist: raw.hasWaitlist ?? false
        };

        const id = this.editingEventId();

        // 82.52: the form has one checkbox, but the two DTOs use distinct property names so an
        // update request that omits the field can't accidentally inherit create's "notify" default.
        if (id) {
            evData.notifyOnUpdate = raw.notifyMembers ?? false;
        } else {
            evData.notifyMembers = raw.notifyMembers ?? true;
        }

        const request = id
            ? this.eventsService.updateEvent(id, evData)
            : this.eventsService.createEvent(evData);

        request.subscribe({
            next: (savedEvent) => {
                const logo = this.selectedLogo();
                if (logo) {
                    this.eventsService.uploadEventLogo(savedEvent.id, logo).subscribe({
                        // 29F.2: surface HTTP failures instead of failing silently
                        next: () => {
                            this.finishSubmission(id ? 'Event updated!' : 'Event created!');
                        },
                        error: () => {
                            this.notify.error('Event saved but logo upload failed.');
                            this.isSubmitting.set(false);
                        }
                    });
                } else {
                    this.finishSubmission(id ? 'Event updated!' : 'Event created!');
                }
            },
            error: (err) => {
                console.error('Event operation failed:', err);
                this.formError.set(err.error?.message || 'Operation failed.');
                this.isSubmitting.set(false);
            }
        });
    }

    private finishSubmission(msg: string) {
        this.notify.success(msg);
        this.showForm.set(false);
        this.isSubmitting.set(false);
        this.loadAllEvents();
    }

    async deleteEvent(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete event',
            message: 'Are you sure you want to delete this event? This action cannot be undone.',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.eventsService.deleteEvent(id).subscribe({
            next: () => {
                this.notify.success('Event removed from system');
                this.loadAllEvents();
            },
            error: () => this.notify.error('Failed to delete event')
        });
    }

    async approveReg(regId: number, approve: boolean) {
        const action = approve ? 'approved' : 'rejected';
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: approve ? 'Approve registration' : 'Reject registration',
            message: `Are you sure you want to ${action} this registration?`,
            confirmLabel: approve ? 'Approve' : 'Reject',
            danger: !approve
        }));
        if (!ok) return;

        this.eventsService.approveRegistration(regId, approve).subscribe({
            next: () => {
                this.notify.success(`Registration successfully ${action}`);
                this.loadAllRegistrations();
            },
            error: (err) => this.notify.error(err.error?.message || 'Action failed')
        });
    }

    viewEvent(ev: AlumniEvent) {
        this.selectedEvent.set(ev);
    }

    closeDetail() {
        this.selectedEvent.set(null);
    }

    fetchInvitation(regId: number) {
        this.eventsService.getRegistrationForInvitation(regId).subscribe({
            next: (data) => {
                this.invitationData.set(data);
                this.showInvitation.set(true);
            },
            error: () => this.notify.error('Could not load invitation data.')
        });
    }

    doPrint() {
        window.print();
    }

    closeInvitation() {
        this.showInvitation.set(false);
        this.invitationData.set(null);
    }

    async sendEmail(regId: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Send invitation',
            message: 'Send invitation email to this participant?',
            confirmLabel: 'Send'
        }));
        if (!ok) return;

        this.eventsService.sendInvitationEmail(regId).subscribe({
            next: () => this.notify.success('Invitation email sent!'),
            error: () => this.notify.error('Failed to send email.')
        });
    }

    openReceipt(path: string) {
        if (!path) return;
        // Prefix with / if not absolute
        const url = path.startsWith('http') || path.startsWith('data:') ? path : '/' + path;
        this.activeReceiptUrl.set(url);
        this.showReceiptModal.set(true);
    }

    closeReceipt() {
        this.showReceiptModal.set(false);
        this.activeReceiptUrl.set(null);
    }
}


