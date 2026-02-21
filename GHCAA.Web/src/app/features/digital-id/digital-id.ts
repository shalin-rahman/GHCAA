import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, MemberProfile } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-digital-id',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="header">
      <h2>Member Credentials</h2>
      <div class="header-btns">
        <button class="btn btn-primary" (click)="download()">Download ID Card</button>
        <button class="btn btn-accent" (click)="downloadCert()">Download Certificate</button>
      </div>
    </div>

    <div class="id-container" *ngIf="profile(); else loading">
      <!-- Front View -->
      <div class="id-card glass-card">
        <div class="card-header">
          <img src="/assets/logo.jpg" alt="Logo" class="logo">
          <div class="header-text">
            <h3>GHCAA</h3>
            <small>ALUMNI ASSOCIATION</small>
          </div>
        </div>
        
        <div class="card-body">
          <div class="photo-placeholder">
            @if (profile()?.photoPath) {
                <img [src]="profile()?.photoPath" alt="Profile" class="profile-img">
            } @else {
                <span>👤</span>
            }
          </div>
          <div class="details">
            <div class="name">{{ profile()?.fullName || 'Member Name' }}</div>
            <div class="role">{{ getMembershipName(profile()?.membershipType) }} Member</div>
            <div class="meta">
              <span><strong>Member ID:</strong> #{{ profile()?.membershipNumber || 'PENDING' }}</span>
              <span><strong>Grad. Year:</strong> {{ profile()?.ghcLastCertificatePassingYear || 'N/A' }}</span>
            </div>
          </div>
        </div>
        
        <div class="card-footer">
          <div class="qr-code">
            <!-- Simulated QR -->
            <div class="qr-box"></div>
            <small>SCAN TO VERIFY</small>
          </div>
          <div class="tagline">Govt. Haraganga College</div>
        </div>
      </div>

      <!-- Back View -->
      <div class="id-card back glass-card">
        <div class="back-content">
          <h4>Terms & Conditions</h4>
          <ul>
            <li>This card is non-transferable.</li>
            <li>Loss must be reported immediately.</li>
            <li>Valid for GHC Alumni events only.</li>
          </ul>
          <div class="expiry">
            <strong>Expiry:</strong> Dec 2030
          </div>
          <div class="seal">
            <img src="/assets/logo.jpg" alt="Seal">
          </div>
        </div>
      </div>
    </div>

    <ng-template #loading>
        <div class="loading">Loading ID card data...</div>
    </ng-template>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .id-container {
      display: flex;
      gap: 3rem;
      flex-wrap: wrap;
      justify-content: center;
      padding: 2rem;
    }
    .id-card {
      width: 350px;
      height: 500px;
      padding: 2rem;
      display: flex;
      flex-direction: column;
      position: relative;
      overflow: hidden;
      background: var(--surface-color);
      border: 1px solid var(--glass-border);
    }
    .card-header {
      display: flex;
      align-items: center;
      gap: 1rem;
      border-bottom: 2px solid var(--accent-color);
      padding-bottom: 1rem;
      margin-bottom: 2rem;
    }
    .logo {
      height: 50px;
      border-radius: 50%;
    }
    .header-text h3 {
      margin: 0;
      color: var(--primary-color);
      letter-spacing: 2px;
    }
    .header-text small {
      font-weight: 700;
      color: var(--text-muted);
    }
    .card-body {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 1.5rem;
    }
    .photo-placeholder {
      width: 140px;
      height: 140px;
      background: var(--bg-color);
      border-radius: 50%;
      border: 4px solid var(--accent-color);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 4rem;
      box-shadow: var(--shadow-md);
      overflow: hidden;
    }
    .profile-img { width: 100%; height: 100%; object-fit: cover; }
    .name {
      font-size: 1.5rem;
      font-weight: 800;
      color: var(--primary-color);
      text-transform: uppercase;
    }
    .role {
      background: var(--accent-color);
      color: white;
      padding: 0.25rem 1rem;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: 700;
    }
    .meta {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      font-size: 0.9rem;
      color: var(--text-muted);
    }
    .card-footer {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      margin-top: auto;
    }
    .qr-box {
      width: 50px;
      height: 50px;
      background: var(--text-main);
      margin-bottom: 0.25rem;
    }
    .tagline {
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--primary-color);
      opacity: 0.6;
    }
    .back-content {
      padding-top: 2rem;
      text-align: center;
    }
    .back h4 {
      margin-bottom: 1.5rem;
      text-decoration: underline;
    }
    .back ul {
      text-align: left;
      font-size: 0.85rem;
      margin-bottom: 2rem;
      color: var(--text-muted);
    }
    .seal img {
      height: 80px;
      opacity: 0.2;
      filter: grayscale(1);
    }
    .loading { text-align: center; padding: 5rem; }

    @media (max-width: 768px) {
      .id-card { width: 300px; height: 430px; }
    }
  `]
})
export class DigitalId implements OnInit {
  private profileService = inject(ProfileService);
  auth = inject(AuthService);

  profile = signal<MemberProfile | null>(null);

  ngOnInit() {
    this.profileService.getProfile().subscribe({
      next: (p) => this.profile.set(p),
      error: () => console.error('Failed to load profile')
    });
  }

  getMembershipName(type: any): string {
    const types = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Life'];
    return types[type] || 'General';
  }

  download() {
    alert('Generating image from ID Card...');
  }

  downloadCert() {
    this.profileService.getCertificate().subscribe(res => {
      if (res.dataUri) {
        const link = document.createElement('a');
        link.href = res.dataUri;
        link.download = 'GHCAA_Certificate.png';
        link.click();
      }
    });
  }
}
