import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../core/services/networking.service';

@Component({
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="governance-page">
      <div class="header">
        <h2>Executive Committee</h2>
        <p>Current leadership of the Govt. Haraganga College Alumni Association</p>
      </div>

      <!-- Committee List -->
      <div class="committee-section">
        @if (loading()) {
            <div class="loading">Loading current committee...</div>
        } @else {
            <div class="committee-grid">
                @for (member of committee(); track member.id) {
                    <div class="glass-card committee-card" [ngClass]="{'executive-head': member.position === 0 || member.position === 1}">
                        <div class="card-content">
                            <div class="avatar-large">{{ member.fullName[0] }}</div>
                            <div class="member-details">
                                <span class="position-badge">{{ getPositionName(member.position) }}</span>
                                <h3>{{ member.fullName }}</h3>
                                <p class="batch">Batch of {{ member.ghcLastCertificatePassingYear }}</p>
                                <div class="professional" *ngIf="member.designation">
                                    <small>{{ member.designation }}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                } @empty {
                    <div class="empty-state glass-card">
                        <p>The Interim Executive Committee (IEC) is currently managing the association operations.</p>
                    </div>
                }
            </div>
        }
      </div>

      <!-- Constitution Quick Reference -->
      <div class="constitution-reference section container">
        <div class="section-header">
            <h2>Governance Pillars</h2>
            <div class="underline"></div>
        </div>
        <div class="pillars-grid">
            <div class="glass-card pillar-card">
                <h4>Political Neutrality</h4>
                <p>The association maintains strict political and religious neutrality to ensure an inclusive environment for all alumni.</p>
            </div>
            <div class="glass-card pillar-card">
                <h4>Life-Long Connection</h4>
                <p>Governance is focused on long-term connection, professional mentorship, and supporting the college's legacy.</p>
            </div>
            <div class="glass-card pillar-card">
                <h4>Transparency</h4>
                <p>All financial records and meeting minutes are archived for member inspection at the annual general meetings.</p>
            </div>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .governance-page { max-width: 1200px; margin: 0 auto; }
    .header { text-align: center; margin-bottom: 4rem; }
    
    .committee-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 2.5rem; justify-content: center; }
    .committee-card { padding: 2.5rem; position: relative; overflow: hidden; }
    .executive-head { border-top: 5px solid var(--accent-color); transform: scale(1.02); }
    
    .card-content { display: flex; gap: 2rem; align-items: flex-start; }
    .avatar-large { width: 80px; height: 80px; background: var(--primary-color); color: white; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 2.5rem; font-weight: 800; }
    
    .position-badge { display: inline-block; background: var(--surface-color); color: var(--primary-color); padding: 0.25rem 0.75rem; border-radius: 4px; font-size: 0.7rem; font-weight: 800; text-transform: uppercase; margin-bottom: 0.75rem; border: 1px solid var(--glass-border); }
    .committee-card h3 { margin: 0; font-size: 1.25rem; color: var(--text-main); }
    .batch { font-size: 0.85rem; color: var(--accent-color); font-weight: 700; margin: 0.25rem 0; }
    .professional { margin-top: 1rem; padding-top: 1rem; border-top: 1px solid var(--glass-border); }
    .professional small { color: var(--text-muted); font-size: 0.8rem; }

    .constitution-reference { margin-top: 6rem; padding-bottom: 4rem; }
    .pillars-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 2rem; margin-top: 3rem; }
    .pillar-card { padding: 2rem; border-left: 4px solid var(--accent-color); background: var(--surface-color); }
    .pillar-card h4 { color: var(--primary-color); margin-bottom: 0.75rem; }
    .pillar-card p { font-size: 0.9rem; color: var(--text-muted); line-height: 1.6; }

    .loading { text-align: center; padding: 4rem; }

    @media (max-width: 992px) {
        .pillars-grid { grid-template-columns: 1fr; }
        .card-content { flex-direction: column; align-items: center; text-align: center; }
        .avatar-large { width: 100px; height: 100px; }
    }
  `]
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);

    committee = signal<any[]>([]);
    loading = signal(true);

    ngOnInit() {
        this.networkService.getCommittee().subscribe({
            next: (data) => {
                this.committee.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getPositionName(pos: any): string {
        const roles = [
            'President',
            'General Secretary',
            'Vice President',
            'Treasurer',
            'Organizational Secretary',
            'Joint Secretary',
            'Information & Tech Secretary',
            'Media & Sports Secretary',
            'Law Secretary',
            'Executive Member'
        ];
        if (typeof pos === 'number') return roles[pos] || 'Executive Member';
        return pos;
    }
}
