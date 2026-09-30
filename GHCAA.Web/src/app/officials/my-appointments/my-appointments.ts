import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, inject, signal } from '@angular/core';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionAppointmentDto } from '../../core/models/election.models';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

@Component({
    selector: 'app-my-appointments',
    standalone: true,
    imports: [CommonModule, FormsModule, LoadingPanelComponent, PageHeaderComponent],
    templateUrl: './my-appointments.html',
    // Keep the line breaks of the persona's declaration.
    styles: ['.declaration-text { white-space: pre-wrap; }']
})
export class MyAppointments {
    private readonly elections = inject(ElectionsService);
    private readonly notify = inject(NotificationService);

    appointments = signal<ElectionAppointmentDto[]>([]);
    loading = signal(true);
    error = signal(false);
    busyId = signal<number | null>(null);
    // Appointment id to the state of its declaration checkbox and decline reason.
    agreed: Record<number, boolean> = {};
    reasons: Record<number, string> = {};

    constructor() {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.elections.getMyAppointments().subscribe({
            next: list => {
                this.appointments.set(list);
                this.error.set(false);
                this.loading.set(false);
            },
            error: () => {
                this.error.set(true);
                this.loading.set(false);
            }
        });
    }

    accept(a: ElectionAppointmentDto): void {
        if (!this.agreed[a.id]) {
            this.notify.error('Tick the declaration first.');
            return;
        }
        this.busyId.set(a.id);
        this.elections.acceptAppointment(a.id).subscribe({
            next: () => {
                this.busyId.set(null);
                this.notify.success(`You are now ${a.personaName} for ${a.electionTitle}.`);
                this.load();
            },
            error: err => {
                this.busyId.set(null);
                this.notify.error(err?.error?.detail || 'The appointment could not be accepted.');
            }
        });
    }

    decline(a: ElectionAppointmentDto): void {
        this.busyId.set(a.id);
        this.elections.declineAppointment(a.id, this.reasons[a.id]?.trim() || null).subscribe({
            next: () => {
                this.busyId.set(null);
                this.appointments.update(list => list.filter(x => x.id !== a.id));
                this.notify.success('Appointment declined.');
            },
            error: err => {
                this.busyId.set(null);
                this.notify.error(err?.error?.detail || 'The appointment could not be declined.');
            }
        });
    }
}
