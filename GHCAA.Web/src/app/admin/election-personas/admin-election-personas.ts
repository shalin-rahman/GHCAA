import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { ElectionPersonasService } from '../../core/services/election-personas.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import {
    ElectionPersonaDto, SaveElectionPersonaDto,
    ELECTION_PERMISSION_FLAGS, ELECTION_PERMISSION_NAMES, ElectionPermissionName
} from '../../core/models/election.models';

const EMPTY_FORM: SaveElectionPersonaDto = {
    name: '',
    groupName: '',
    description: '',
    permissions: 0,
    minCount: 0,
    maxCount: null,
    showOnPublicBoard: true,
    takesOverFromAdmin: false,
    declarationText: '',
    sortOrder: 0
};

// Spec 023 (37.12b). SuperAdmin screen for the ElectionPersona table: who can do what during
// an election, and what each of them signs before taking on the role.
@Component({
    selector: 'app-admin-election-personas',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, SearchBarComponent, ModalHeaderComponent],
    templateUrl: './admin-election-personas.html',
    styleUrl: './admin-election-personas.scss'
})
export class AdminElectionPersonas implements OnInit {
    private service = inject(ElectionPersonasService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

    personas = signal<ElectionPersonaDto[]>([]);
    loading = signal(true);
    search = signal('');

    showModal = signal(false);
    editId = signal<number | null>(null);
    form = signal<SaveElectionPersonaDto>({ ...EMPTY_FORM });
    isSaving = signal(false);

    permissionNames: ElectionPermissionName[] = ELECTION_PERMISSION_NAMES;

    filtered = computed(() => {
        const q = this.search().toLowerCase();
        return this.personas().filter(p =>
            p.name.toLowerCase().includes(q) || p.groupName.toLowerCase().includes(q)
        );
    });

    ngOnInit() {
        this.load();
    }

    load() {
        this.loading.set(true);
        this.service.list().subscribe({
            next: data => {
                this.personas.set(data);
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Failed to load election personas.');
                this.loading.set(false);
            }
        });
    }

    hasPermission(flag: ElectionPermissionName): boolean {
        return (this.form().permissions & ELECTION_PERMISSION_FLAGS[flag]) !== 0;
    }

    togglePermission(flag: ElectionPermissionName, checked: boolean) {
        const bit = ELECTION_PERMISSION_FLAGS[flag];
        this.form.update(f => ({ ...f, permissions: checked ? f.permissions | bit : f.permissions & ~bit }));
    }

    openNew() {
        this.editId.set(null);
        this.form.set({ ...EMPTY_FORM });
        this.showModal.set(true);
    }

    openEdit(p: ElectionPersonaDto) {
        this.editId.set(p.id);
        this.form.set({
            name: p.name,
            groupName: p.groupName,
            description: p.description,
            permissions: p.permissions,
            minCount: p.minCount,
            maxCount: p.maxCount,
            showOnPublicBoard: p.showOnPublicBoard,
            takesOverFromAdmin: p.takesOverFromAdmin,
            declarationText: p.declarationText,
            sortOrder: p.sortOrder
        });
        this.showModal.set(true);
    }

    save() {
        const data = this.form();
        if (!data.name || !data.groupName || !data.description || !data.declarationText) {
            this.notify.error('Name, group, description and declaration text are required.');
            return;
        }

        this.isSaving.set(true);
        const id = this.editId();
        const request = id ? this.service.update(id, data) : this.service.create(data);
        request.subscribe({
            next: () => {
                this.notify.success(id ? 'Persona updated' : 'Persona created');
                this.showModal.set(false);
                this.isSaving.set(false);
                this.load();
            },
            error: (err) => {
                this.notify.error(err.error?.detail || (id ? 'Failed to update persona.' : 'Failed to create persona.'));
                this.isSaving.set(false);
            }
        });
    }

    toggleActive(p: ElectionPersonaDto) {
        this.service.setActive(p.id, !p.isActive).subscribe({
            next: () => {
                this.notify.success(p.isActive ? 'Persona deactivated' : 'Persona activated');
                this.load();
            },
            error: () => this.notify.error('Failed to change persona status.')
        });
    }

    async remove(p: ElectionPersonaDto) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete persona',
            message: `Delete "${p.name}"? This cannot be undone.`,
            confirmLabel: 'Delete'
        }));
        if (!ok) return;

        this.service.delete(p.id).subscribe({
            next: () => {
                this.notify.success('Persona deleted');
                this.load();
            },
            error: (err) => this.notify.error(err.error?.detail || 'Failed to delete persona.')
        });
    }
}
