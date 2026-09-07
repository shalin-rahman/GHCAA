import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { SiteContentService } from '../../core/services/site-content.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { SiteContent, UpsertSiteContentDto } from '../../core/models/business.models';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { RichTextEditor } from '../../common/rich-text-editor/rich-text-editor';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

const emptyForm = (): UpsertSiteContentDto => ({
    key: '',
    group: 'about',
    title: '',
    bodyHtml: '',
    displayOrder: 0,
    isActive: true
});

@Component({
    selector: 'app-admin-site-content',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, SearchBarComponent, RichTextEditor, LogoSpinnerComponent],
    templateUrl: './site-content.html',
    styleUrl: './site-content.scss'
})
export class AdminSiteContent implements OnInit {
    private service = inject(SiteContentService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

    blocks = signal<SiteContent[]>([]);
    isLoading = signal(false);
    isSaving = signal(false);
    showForm = signal(false);
    editingId = signal<number | null>(null);
    searchQuery = signal('');

    form: UpsertSiteContentDto = emptyForm();

    readonly groups = ['about', 'contact', 'home', 'footer'];

    filteredBlocks = computed(() => {
        const q = this.searchQuery().toLowerCase().trim();
        const list = this.blocks();
        if (!q) return list;
        return list.filter(b =>
            b.title.toLowerCase().includes(q) ||
            b.key.toLowerCase().includes(q) ||
            b.group.toLowerCase().includes(q));
    });

    ngOnInit() {
        this.load();
    }

    load() {
        this.isLoading.set(true);
        this.service.getAll().subscribe({
            next: list => {
                this.blocks.set([...list].sort((a, b) =>
                    a.group.localeCompare(b.group) || a.displayOrder - b.displayOrder));
                this.isLoading.set(false);
            },
            error: () => this.isLoading.set(false)
        });
    }

    openForm() {
        this.form = emptyForm();
        this.editingId.set(null);
        this.showForm.set(true);
    }

    editBlock(block: SiteContent) {
        this.form = {
            key: block.key,
            group: block.group,
            title: block.title,
            bodyHtml: block.bodyHtml,
            displayOrder: block.displayOrder,
            isActive: block.isActive
        };
        this.editingId.set(block.id);
        this.showForm.set(true);
    }

    cancelForm() {
        this.showForm.set(false);
        this.editingId.set(null);
    }

    save(ngForm: NgForm) {
        if (ngForm.invalid) {
            this.notify.error('Please complete the required fields.');
            return;
        }
        this.isSaving.set(true);
        const id = this.editingId();
        const request = id ? this.service.update(id, this.form) : this.service.create(this.form);
        request.subscribe({
            next: () => {
                this.notify.success(id ? 'Content block updated.' : 'Content block created.');
                this.isSaving.set(false);
                this.showForm.set(false);
                this.editingId.set(null);
                this.load();
            },
            error: () => this.isSaving.set(false)
        });
    }

    toggleActive(block: SiteContent) {
        this.service.update(block.id, {
            key: block.key,
            group: block.group,
            title: block.title,
            bodyHtml: block.bodyHtml,
            displayOrder: block.displayOrder,
            isActive: !block.isActive
        }).subscribe(() => this.load());
    }

    async deleteBlock(block: SiteContent) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete content block',
            message: `Delete content block "${block.title}"? This cannot be undone.`,
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.service.delete(block.id).subscribe(() => {
            this.notify.success('Content block deleted.');
            this.load();
        });
    }
}
