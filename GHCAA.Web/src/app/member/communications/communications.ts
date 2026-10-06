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
      <div class="admin-feature">
        <app-page-header title="My communications" emptySubtitle="Email and SMS delivery history"></app-page-header>
        <app-loading-panel *ngIf="loading()" label="Loading communications"></app-loading-panel>
        <ng-container *ngIf="!loading()">
          <div class="empty-state" *ngIf="logs().length === 0"><p>No communications have been recorded.</p></div>
          <div class="table-wrap" *ngIf="logs().length > 0">
            <table class="data-table">
              <thead>
                <tr><th>Message</th><th>Channel</th><th>Status</th><th>Date</th></tr>
              </thead>
              <tbody>
                <tr *ngFor="let log of logs()">
                  <td>
                    <strong>{{ log.subject || log.channel || 'Communication' }}</strong>
                    <div class="text-muted">{{ log.body }}</div>
                  </td>
                  <td>{{ log.channel || 'Email' }} · {{ log.deliveryScope || 'Targeted' }}</td>
                  <td>{{ log.status }}</td>
                  <td>{{ log.sentDate | appDate }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <button class="btn btn-outline btn-sm" *ngIf="hasMore()" (click)="loadMore()">Load more</button>
        </ng-container>
      </div>
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
