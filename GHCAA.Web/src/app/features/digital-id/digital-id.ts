import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, MemberProfile } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-digital-id',
  standalone: true,
  imports: [CommonModule],
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
    this.notify.info('Preparing your high-resolution ID card image. Please wait...');
    // In a real app, use html2canvas or similar
    setTimeout(() => {
      this.notify.success('ID Card generated successfully! Download started.');
    }, 2000);
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
