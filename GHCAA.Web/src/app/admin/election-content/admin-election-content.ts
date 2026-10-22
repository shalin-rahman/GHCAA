import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { ElectionContentService } from '../../core/services/election-content.service';
import { NotificationService } from '../../core/services/notification.service';
import {
    ElectionContentDto, ElectionContentKind, ElectionPageFormat, UpsertElectionContentDto, ELECTION_PAGE_FORMATS
} from '../../core/models/election.models';

const EMPTY_FORM: UpsertElectionContentDto = {
    key: '',
    kind: 'Page',
    format: 'Prose',
    group: '',
    title: '',
    summary: '',
    body: '',
    displayOrder: 0,
    isPublished: true
};

// 37.14s. Admin screen for the ElectionContent table: the election pages on the public
// Elections screen and the layouts the server prints forms from. A body may hold
// {{tokens}} such as {{org.fullName}}, which the server fills in when it sends the row out.
@Component({
    selector: 'app-admin-election-content',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, SearchBarComponent, ModalHeaderComponent],
    templateUrl: './admin-election-content.html',
    styleUrl: './admin-election-content.scss'
})
export class AdminElectionContent implements OnInit {
    private service = inject(ElectionContentService);
    private notify = inject(NotificationService);

    rows = signal<ElectionContentDto[]>([]);
    loading = signal(true);
    search = signal('');
    kindFilter = signal<ElectionContentKind | ''>('');

    showModal = signal(false);
    editId = signal<number | null>(null);
    form = signal<UpsertElectionContentDto>({ ...EMPTY_FORM });
    loadingBody = signal(false);
    isSaving = signal(false);

    formats: ElectionPageFormat[] = ELECTION_PAGE_FORMATS;

    filtered = computed(() => {
        const q = this.search().toLowerCase();
        const kind = this.kindFilter();
        return this.rows().filter(r =>
            (!kind || r.kind === kind) &&
            (r.key.toLowerCase().includes(q) || r.title.toLowerCase().includes(q) || (r.group ?? '').toLowerCase().includes(q))
        );
    });

    ngOnInit() {
        this.load();
    }

    load() {
        this.loading.set(true);
        this.service.adminList().subscribe({
            next: data => {
                this.rows.set(data);
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Failed to load election content.');
                this.loading.set(false);
            }
        });
    }

    openNew() {
        this.editId.set(null);
        this.form.set({ ...EMPTY_FORM });
        this.showModal.set(true);
    }

    // The list carries no bodies, so the stored text is fetched when the editor opens.
    openEdit(row: ElectionContentDto) {
        this.editId.set(row.id);
        this.form.set(this.toForm(row));
        this.loadingBody.set(true);
        this.showModal.set(true);
        this.service.adminGet(row.id).subscribe({
            next: full => {
                this.form.set(this.toForm(full));
                this.loadingBody.set(false);
            },
            error: (err) => {
                this.notify.error(err.error?.detail || 'Failed to load this entry.');
                this.showModal.set(false);
                this.loadingBody.set(false);
            }
        });
    }

    setKind(kind: ElectionContentKind) {
        // A page needs a format; a form has none.
        this.form.update(f => ({ ...f, kind, format: kind === 'Page' ? (f.format ?? 'Prose') : null }));
    }

    save() {
        const data = this.form();
        if (!data.key.trim() || !data.title.trim() || !data.body.trim()) {
            this.notify.error('Key, title and body are required.');
            return;
        }
        if (!/^[A-Za-z0-9-]+$/.test(data.key)) {
            this.notify.error('Key may hold only letters, digits and hyphens.');
            return;
        }
        if (data.kind === 'Form' && !this.isJson(data.body)) {
            this.notify.error('A form body must be valid JSON.');
            return;
        }

        const request: UpsertElectionContentDto = {
            ...data,
            key: data.key.trim(),
            format: data.kind === 'Page' ? data.format : null,
            group: data.group?.trim() || null,
            summary: data.summary?.trim() || null
        };
        this.isSaving.set(true);
        const id = this.editId();
        const call = id ? this.service.update(id, request) : this.service.create(request);
        call.subscribe({
            next: () => {
                this.notify.success(id ? 'Entry updated' : 'Entry created');
                this.showModal.set(false);
                this.isSaving.set(false);
                this.load();
            },
            error: (err) => {
                this.notify.error(err.error?.detail || (id ? 'Failed to update entry.' : 'Failed to create entry.'));
                this.isSaving.set(false);
            }
        });
    }

    private toForm(row: ElectionContentDto): UpsertElectionContentDto {
        return {
            key: row.key,
            kind: row.kind,
            format: row.format,
            group: row.group ?? '',
            title: row.title,
            summary: row.summary ?? '',
            body: row.body ?? '',
            displayOrder: row.displayOrder,
            isPublished: row.isPublished
        };
    }

    private isJson(text: string): boolean {
        try {
            JSON.parse(text);
            return true;
        } catch {
            return false;
        }
    }
}
