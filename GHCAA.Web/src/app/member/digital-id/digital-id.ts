import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../../core/services/profile.service';
import { MemberProfile } from '../../core/models/business.models';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
  selector: 'app-digital-id',
  standalone: true,
  imports: [CommonModule, LogoSpinnerComponent],
  templateUrl: './digital-id.html',
  styleUrl: './digital-id.scss'
})
export class DigitalId implements OnInit {
  private profileService = inject(ProfileService);
  auth = inject(AuthService);
  private notify = inject(NotificationService);

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
    // 29D.5: Call the real server-rendered ID card (was a fake setTimeout that claimed
    // success without producing any file).
    this.notify.info('Preparing your high-resolution ID card image. Please wait...');
    this.profileService.getIDCard().subscribe({
      next: (res) => {
        if (res.dataUri) {
          const link = document.createElement('a');
          link.href = res.dataUri;
          link.download = 'GHCAA_ID_Card.png';
          link.click();
          this.notify.success('ID Card downloaded.');
        } else {
          this.notify.error('ID card is not available yet.');
        }
      },
      error: () => this.notify.error('Failed to generate ID card. Please try again.')
    });
  }

  downloadCert() {
    this.profileService.getCertificate().subscribe({
      next: (res) => {
        if (res.dataUri) {
          const link = document.createElement('a');
          link.href = res.dataUri;
          link.download = 'GHCAA_Certificate.png';
          link.click();
        } else {
          this.notify.error('Certificate is not available yet.');
        }
      },
      error: () => this.notify.error('Failed to download certificate. Please try again.')
    });
  }
}


