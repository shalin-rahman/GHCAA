import { CommonModule } from '@angular/common';
import { Component, Input, OnDestroy, OnInit, computed, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ElectionsService } from '../../../core/services/elections.service';
import { ElectionPersonasService } from '../../../core/services/election-personas.service';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { debounce } from '../../../core/utils/debounce.util';
import { ELECTION_APPOINTMENT_REASON_MAX, SEARCH_DEBOUNCE_MS, SUPER_ADMIN_ROLE } from '../../../core/constants/app.constants';
import {
    AdminElectionDto, AppointRequest, ElectionApprovalDto, ElectionAppointmentDto, ElectionPersonaDto
} from '../../../core/models/election.models';

interface MemberHit { id: number; fullName: string; membershipNumber: string | null; }

// 37.13y. Officials of one election: list, appoint, revoke, and the SuperAdmin emergency revoke.
// The server holds the rules (frozen rules, one Returning Officer, permissions); this panel only
// shows its answer, which the HTTP interceptor already turns into a toast on failure.
@Component({
    selector: 'app-election-officials',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './election-officials.html'
})
export class ElectionOfficials implements OnInit, OnDestroy {
    private readonly electionsService = inject(ElectionsService);
    private readonly personasService = inject(ElectionPersonasService);
    private readonly adminService = inject(AdminService);
    private readonly auth = inject(AuthService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);

    @Input({ required: true }) election!: AdminElectionDto;
    // An emergency revoke is stored for a second person; the page adds it to the approvals list.
    pending = output<ElectionApprovalDto>();

    readonly reasonMax = ELECTION_APPOINTMENT_REASON_MAX;
    readonly isSuperAdmin = computed(() => this.auth.hasRole(SUPER_ADMIN_ROLE));

    appointments = signal<ElectionAppointmentDto[]>([]);
    personas = signal<ElectionPersonaDto[]>([]);
    loading = signal(true);
    saving = signal(false);
    busyId = signal<number | null>(null);

    // Revoked rows stay in the list from the server for the record; only open ones get buttons.
    open = computed(() => this.appointments().filter(a => !a.revokedAt));
    closed = computed(() => this.appointments().filter(a => a.revokedAt));

    memberQuery = signal('');
    memberHits = signal<MemberHit[]>([]);
    pickedMember = signal<MemberHit | null>(null);
    form = this.emptyForm();

    emergencyId = signal<number | null>(null);
    emergencyReason = '';

    ngOnInit(): void {
        this.load();
        this.personasService.listActive().subscribe({
            next: values => this.personas.set(values),
            error: () => this.notify.error('Failed to load election roles.')
        });
    }

    ngOnDestroy(): void {
        this.debouncedSearch.cancel();
    }

    load(): void {
        this.loading.set(true);
        this.electionsService.getAppointments(this.election.id).subscribe({
            next: values => { this.appointments.set(values); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    private emptyForm() {
        return { personaId: 0, displayName: '', email: '', phone: '', isReturningOfficer: false };
    }

    private readonly debouncedSearch = debounce((query: string) => this.runSearch(query), SEARCH_DEBOUNCE_MS);

    searchMembers(query: string): void {
        this.memberQuery.set(query);
        this.pickedMember.set(null);
        if (query.trim().length < 2) {
            this.memberHits.set([]);
            return;
        }
        this.debouncedSearch(query.trim());
    }

    private runSearch(query: string): void {
        this.adminService.getMembers(1, 10, query).subscribe({
            next: (response: { items?: MemberHit[] } | MemberHit[]) => {
                // A slower reply for an older query must not replace the newer list.
                if (this.memberQuery().trim() !== query) return;
                this.memberHits.set(Array.isArray(response) ? response : response.items ?? []);
            },
            error: () => this.memberHits.set([])
        });
    }

    pickMember(member: MemberHit): void {
        this.pickedMember.set(member);
        this.memberQuery.set(member.fullName);
        this.memberHits.set([]);
    }

    clearMember(): void {
        this.pickedMember.set(null);
        this.memberQuery.set('');
    }

    // Mirrors AppointDto.Validate: a member, or a name and email, never both.
    buildRequest(): AppointRequest | string {
        if (!this.form.personaId) return 'Choose a role.';
        const member = this.pickedMember();
        const name = this.form.displayName.trim();
        const email = this.form.email.trim();
        if (!member && (!name || !email)) return 'Choose a member, or give a name and email.';
        return {
            personaId: this.form.personaId,
            memberId: member?.id ?? null,
            displayName: member ? null : name,
            email: member ? null : email,
            phone: member ? null : (this.form.phone.trim() || null),
            isReturningOfficer: this.form.isReturningOfficer
        };
    }

    appoint(): void {
        if (this.saving()) return;
        const request = this.buildRequest();
        if (typeof request === 'string') {
            this.notify.warning(request);
            return;
        }
        this.saving.set(true);
        this.electionsService.appoint(this.election.id, request).subscribe({
            next: appointment => {
                this.saving.set(false);
                this.appointments.update(all => [...all, appointment]);
                this.form = this.emptyForm();
                this.clearMember();
                this.notify.success(`${appointment.displayName} appointed. They must accept before the role takes effect.`);
            },
            error: () => this.saving.set(false)
        });
    }

    async revoke(appointment: ElectionAppointmentDto): Promise<void> {
        if (this.busyId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Revoke appointment',
            message: `Remove ${appointment.displayName} as ${appointment.personaName}?`,
            confirmLabel: 'Revoke',
            danger: true
        }));
        if (!confirmed) return;

        this.busyId.set(appointment.id);
        this.electionsService.revokeAppointment(appointment.id, null).subscribe({
            next: () => { this.busyId.set(null); this.notify.success('Appointment revoked.'); this.load(); },
            error: () => this.busyId.set(null)
        });
    }

    startEmergency(appointment: ElectionAppointmentDto): void {
        this.emergencyId.set(appointment.id);
        this.emergencyReason = '';
    }

    cancelEmergency(): void {
        this.emergencyId.set(null);
        this.emergencyReason = '';
    }

    requestEmergency(appointment: ElectionAppointmentDto): void {
        const reason = this.emergencyReason.trim();
        if (!reason) {
            this.notify.warning('Say why this official has to go.');
            return;
        }
        if (this.busyId() !== null) return;
        this.busyId.set(appointment.id);
        this.electionsService.emergencyRevoke(appointment.id, reason).subscribe({
            next: approval => {
                this.busyId.set(null);
                this.cancelEmergency();
                this.pending.emit(approval);
                this.notify.info('Saved. A second person must approve the removal before it happens.');
            },
            error: () => this.busyId.set(null)
        });
    }
}
