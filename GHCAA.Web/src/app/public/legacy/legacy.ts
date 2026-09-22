import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ArchiveService } from '../../core/services/archive.service';
import { ArchiveCollection, ArchiveItem } from '../../core/models/archive.models';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';

@Component({
    selector: 'app-legacy',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, LoadingPanelComponent],
    templateUrl: './legacy.html',
    styleUrl: './legacy.scss'
})
export class LegacyPage implements OnInit {
    private service = inject(ArchiveService);
    private route = inject(ActivatedRoute);

    loading = signal(true);
    error = signal(false);
    collections = signal<ArchiveCollection[]>([]);
    item = signal<ArchiveItem | null>(null);
    search = '';
    itemId = signal<number | null>(null);

    ngOnInit() {
        const id = Number(this.route.snapshot.paramMap.get('id'));
        if (Number.isInteger(id) && id > 0) {
            this.itemId.set(id);
            this.service.getPublicItem(id).subscribe({
                next: item => { this.item.set(item); this.loading.set(false); },
                error: () => { this.error.set(true); this.loading.set(false); }
            });
            return;
        }
        this.loadCollections();
    }

    loadCollections() {
        this.loading.set(true);
        this.error.set(false);
        this.service.getPublicCollections(this.search).subscribe({
            next: collections => { this.collections.set(collections); this.loading.set(false); },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }

    retry() {
        if (this.itemId()) {
            this.loading.set(true);
            this.error.set(false);
            this.service.getPublicItem(this.itemId()!).subscribe({
                next: item => { this.item.set(item); this.loading.set(false); },
                error: () => { this.error.set(true); this.loading.set(false); }
            });
        } else {
            this.loadCollections();
        }
    }
}
