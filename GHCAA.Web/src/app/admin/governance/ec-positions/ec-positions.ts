import { Component, ChangeDetectionStrategy, inject, signal, OnInit, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, firstValueFrom } from 'rxjs';
import { ModalHeaderComponent } from '../../../common/modal-header/modal-header.component';
import { EcPositionService, EcPosition, SaveEcPositionRequest, InvalidPositionRow } from '../../../core/services/ec-position.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

// Match Range and MaxLength on SaveEcPositionRequest in GovernanceDto.cs.
export const EC_POSITION_MAX_SEATS = 20;
export const EC_POSITION_NAME_MAX_LENGTH = 100;
export const EC_POSITION_MAX_DISPLAY_ORDER = 999;

const emptyForm = (): SaveEcPositionRequest => ({
    name: '', nameBn: '', displayOrder: 0, seatCount: 1,
    showOnBoard: false, signsAsHead: false, signsAsSecretary: false
});

// 95.2: admins add, rename, retire and delete committee positions here.
// The API refuses a delete for a position that has ever been held, and says so in the 409 detail.
@Component({
    selector: 'app-ec-positions',
    standalone: true,
    imports: [FormsModule, ModalHeaderComponent],
    templateUrl: './ec-positions.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    host: { '(document:keydown.escape)': 'onEscape()' }
})
export class EcPositionsComponent implements OnInit {
    private positionService = inject(EcPositionService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

    closed = output<void>();

    readonly maxSeats = EC_POSITION_MAX_SEATS;
    readonly nameMax = EC_POSITION_NAME_MAX_LENGTH;
    readonly maxDisplayOrder = EC_POSITION_MAX_DISPLAY_ORDER;
    positions = signal<EcPosition[]>([]);
    loading = signal(false);
    saving = signal(false);
    // null means the list is showing; 0 means a new position; otherwise the id being edited.
    editingId = signal<number | null>(null);
    form = signal<SaveEcPositionRequest>(emptyForm());
    invalidRows = signal<InvalidPositionRow[]>([]);
    // Position picked for each invalid row, keyed by kind and id.
    repairChoice = signal<Partial<Record<string, number>>>({});

    ngOnInit() {
        this.load();
    }

    load() {
        this.loadInvalidRows();
        this.loading.set(true);
        this.positionService.getAll().subscribe({
            next: list => {
                this.positions.set([...list].sort((a, b) => a.displayOrder - b.displayOrder || a.id - b.id));
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Could not load positions.');
                this.loading.set(false);
            }
        });
    }

    // Only rows left by the upgrade show up here, so an empty list hides the section.
    loadInvalidRows() {
        this.positionService.getInvalidRows().subscribe({
            next: rows => this.invalidRows.set(rows),
            error: () => this.invalidRows.set([])
        });
    }

    rowKey(row: InvalidPositionRow): string {
        return `${row.kind}:${row.id}`;
    }

    pickRepair(row: InvalidPositionRow, positionId: number) {
        this.repairChoice.update(c => ({ ...c, [this.rowKey(row)]: positionId }));
    }

    async repair(row: InvalidPositionRow) {
        const positionId = this.repairChoice()[this.rowKey(row)];
        if (!positionId) {
            this.notify.error('Pick a position first.');
            return;
        }
        await this.run(() => this.positionService.repair(row, positionId), 'Row repaired.', 'Could not repair the row.');
    }

    onEscape() {
        if (this.editingId() !== null) this.cancelEdit();
        else this.closed.emit();
    }

    openNew() {
        const next = this.positions().reduce((max, p) => Math.max(max, p.displayOrder), 0) + 1;
        this.form.set({ ...emptyForm(), displayOrder: next });
        this.editingId.set(0);
    }

    openEdit(p: EcPosition) {
        this.form.set({
            name: p.name, nameBn: p.nameBn ?? '', displayOrder: p.displayOrder, seatCount: p.seatCount,
            showOnBoard: p.showOnBoard, signsAsHead: p.signsAsHead, signsAsSecretary: p.signsAsSecretary
        });
        this.editingId.set(p.id);
    }

    cancelEdit() {
        this.editingId.set(null);
    }

    formError(): string | null {
        const f = this.form();
        if (!f.name?.trim()) return 'Name is required.';
        if (!Number.isInteger(f.displayOrder) || f.displayOrder < 0 || f.displayOrder > this.maxDisplayOrder) {
            return `Display order must be a whole number from 0 to ${this.maxDisplayOrder}.`;
        }
        if (!Number.isInteger(f.seatCount) || f.seatCount < 1 || f.seatCount > this.maxSeats) {
            return `Seats must be a whole number from 1 to ${this.maxSeats}.`;
        }
        return null;
    }

    async save() {
        const error = this.formError();
        if (error) {
            this.notify.error(error);
            return;
        }
        const f = this.form();
        const body: SaveEcPositionRequest = { ...f, name: f.name.trim(), nameBn: f.nameBn?.trim() || null };
        const id = this.editingId();
        this.saving.set(true);
        try {
            if (id) await firstValueFrom(this.positionService.update(id, body));
            else await firstValueFrom(this.positionService.create(body));
            this.notify.success(id ? 'Position updated.' : 'Position added.');
            this.editingId.set(null);
            this.load();
        } catch (err: any) {
            this.notify.error(err?.error?.detail || 'Could not save the position.');
        } finally {
            this.saving.set(false);
        }
    }

    async retire(p: EcPosition) {
        await this.run(() => this.positionService.retire(p.id), `${p.name} retired.`, 'Could not retire the position.');
    }

    async restore(p: EcPosition) {
        await this.run(() => this.positionService.restore(p.id), `${p.name} restored.`, 'Could not restore the position.');
    }

    async remove(p: EcPosition) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete Position',
            message: `Delete ${p.name}? A position that has ever been held cannot be deleted; retire it instead.`,
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;
        await this.run(() => this.positionService.remove(p.id), `${p.name} deleted.`, 'Could not delete the position.');
    }

    patch(changes: Partial<SaveEcPositionRequest>) {
        this.form.update(f => ({ ...f, ...changes }));
    }

    private async run(call: () => Observable<unknown>, done: string, fallback: string) {
        try {
            await firstValueFrom(call());
            this.notify.success(done);
            this.load();
        } catch (err: any) {
            this.notify.error(err?.error?.detail || fallback);
        }
    }
}
