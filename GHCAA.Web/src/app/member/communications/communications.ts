import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { MemberCommunicationsService } from '../../core/services/member-communications.service';
import { EmailLog } from '../../core/services/admin-comm.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

@Component({
    selector: 'app-member-communications',
    standalone: true,
    imports: [CommonModule, AppDatePipe, LoadingPanelComponent, PageHeaderComponent],
    template: `
      <app-page-header title="My communications" subtitle="Email and SMS delivery history"></app-page-header>
      <app-loading-panel *ngIf="loading()"></app-loading-panel>
      <section class="glass-card" *ngIf="!loading()">
        <div class="empty-state" *ngIf="logs().length === 0">No communications have been recorded.</div>
        <article class="communication-row" *ngFor="let log of logs()">
          <div>
            <strong>{{ log.subject || log.channel || 'Communication' }}</strong>
            <p>{{ log.body }}</p>
          </div>
          <div class="communication-meta">
            <span>{{ log.channel || 'Email' }} · {{ log.deliveryScope || 'Targeted' }}</span>
            <span>{{ log.status }} · {{ log.sentDate | appDate }}</span>
          </div>
        </article>
        <button class="btn btn-outline btn-sm" *ngIf="hasMore()" (click)="loadMore()">Load more</button>
      </section>
    `
})
export class MemberCommunications {
    private service = inject(MemberCommunicationsService);
    logs = signal<EmailLog[]>([]);
    loading = signal(true);
    private page = 1;
    private readonly pageSize = 25;
    hasMore = signal(false);

    constructor() { this.load(); }

    private load(): void {
        this.service.getMine(this.page, this.pageSize).subscribe({
            next: result => {
                this.logs.update(items => [...items, ...result.items]);
                this.hasMore.set(result.page * result.pageSize < result.totalCount);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    loadMore(): void {
        this.page++;
        this.load();
    }
}
