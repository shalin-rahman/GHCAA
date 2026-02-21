import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-directory',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="directory-page">
      <div class="header">
        <h2>Member Directory</h2>
        <p>Find and connect with fellow Haragangians</p>
      </div>

      <!-- Search & Filters -->
      <div class="glass-card filters">
        <div class="filter-grid">
          <div class="filter-group">
            <label>Name / Email</label>
            <input type="text" [(ngModel)]="filters.query" (input)="search()" placeholder="Search by name...">
          </div>
          <div class="filter-group">
            <label>Batch / Year</label>
            <input type="number" [(ngModel)]="filters.year" (change)="search()" placeholder="Passing Year">
          </div>
          <div class="filter-group">
            <label>Professional Sector</label>
            <select [(ngModel)]="filters.sector" (change)="search()">
                <option value="">All Sectors</option>
                <option value="Govt. Service">Govt. Service</option>
                <option value="Corporate">Corporate</option>
                <option value="Business">Business</option>
                <option value="Education">Education</option>
                <option value="Medical">Medical</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Results -->
      <div class="member-list">
        @if (loading()) {
            <div class="loading">Searching directory...</div>
        } @else {
            <div class="grid-container">
                @for (m of members(); track m.id) {
                    <div class="glass-card member-card">
                        <div class="member-header">
                            <div class="avatar">{{ m.fullName[0] }}</div>
                            <div class="name-info">
                                <h4>{{ m.fullName }}</h4>
                                <span class="badge">{{ m.membershipNumber || 'Member' }}</span>
                            </div>
                        </div>
                        <div class="member-body">
                            <div class="info-item">
                                <span class="icon">🎓</span>
                                <span>Batch of {{ m.ghcLastCertificatePassingYear }}</span>
                            </div>
                            <div class="info-item" *ngIf="m.professionalSector">
                                <span class="icon">💼</span>
                                <span>{{ m.designation }} at {{ m.professionalSector }}</span>
                            </div>
                            <div class="info-item" *ngIf="m.isEmailPublic && m.email">
                                <span class="icon">📧</span>
                                <a [href]="'mailto:' + m.email">{{ m.email }}</a>
                            </div>
                        </div>
                        <div class="card-footer">
                            <button class="btn btn-accent btn-sm" (click)="viewProfile(m.id)">View Full Profile</button>
                        </div>
                    </div>
                } @empty {
                    <div class="empty-state glass-card">
                        <p>No members found matching your search.</p>
                    </div>
                }
            </div>
        }
      </div>
    </div>
  `,
    styles: [`
    .directory-page { max-width: 1200px; margin: 0 auto; }
    .header { margin-bottom: 2.5rem; }
    
    .filters { padding: 2rem; margin-bottom: 3rem; }
    .filter-grid { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 2rem; }
    .filter-group label { display: block; font-size: 0.75rem; font-weight: 700; margin-bottom: 0.5rem; color: var(--text-muted); }
    .filter-group input, .filter-group select { width: 100%; padding: 0.8rem; border: 1px solid var(--glass-border); border-radius: 8px; background: var(--bg-color); color: var(--text-main); }

    .grid-container { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 2rem; }
    .member-card { padding: 2rem; transition: 0.3s; display: flex; flex-direction: column; gap: 1.5rem; }
    .member-card:hover { transform: translateY(-5px); border-color: var(--primary-color); }
    
    .member-header { display: flex; gap: 1rem; align-items: center; }
    .avatar { width: 50px; height: 50px; background: var(--primary-color); color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 1.2rem; }
    .name-info h4 { margin: 0; color: var(--text-main); }
    .badge { font-size: 0.65rem; background: var(--surface-color); padding: 0.2rem 0.5rem; border-radius: 4px; font-weight: 700; color: var(--text-muted); border: 1px solid var(--glass-border); }

    .member-body { display: flex; flex-direction: column; gap: 0.5rem; flex: 1; }
    .info-item { display: flex; gap: 0.75rem; font-size: 0.85rem; color: var(--text-muted); align-items: center; }
    .info-item .icon { font-size: 1rem; }
    .info-item a { color: var(--primary-color); text-decoration: none; font-weight: 600; }

    .card-footer { padding-top: 1rem; border-top: 1px solid var(--glass-border); }
    .loading { text-align: center; padding: 4rem; }

    @media (max-width: 768px) {
        .filter-grid { grid-template-columns: 1fr; gap: 1rem; }
    }
  `]
})
export class Directory implements OnInit {
    private networkService = inject(NetworkingService);
    private notify = inject(NotificationService);

    members = signal<any[]>([]);
    loading = signal(true);

    filters = {
        query: '',
        year: null,
        sector: ''
    };

    ngOnInit() {
        this.search();
    }

    search() {
        this.loading.set(true);
        this.networkService.searchMembers(this.filters).subscribe({
            next: (data) => {
                this.members.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    viewProfile(id: number) {
        this.notify.info('Full profile view and direct messaging coming in the next update!');
    }
}
