import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CredentialVerificationService, CredentialVerification } from '../../core/services/credential-verification.service';

@Component({
  selector: 'app-credential-verification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verify.html',
  styleUrl: './verify.scss'
})
export class Verify implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(CredentialVerificationService);
  shortCode = '';
  result = signal<CredentialVerification | null>(null);
  loading = signal(false);
  error = signal('');

  ngOnInit(): void {
    const code = this.route.snapshot.paramMap.get('shortCode');
    if (code) {
      this.shortCode = code;
      this.verify();
    }
  }

  verify(): void {
    this.error.set('');
    this.result.set(null);
    if (!/^[A-HJ-NP-Z2-9]{10}$/i.test(this.shortCode.trim())) {
      this.error.set('Enter a valid credential code.');
      return;
    }
    this.loading.set(true);
    this.service.verify(this.shortCode).subscribe({
      next: value => { this.result.set(value); this.loading.set(false); },
      error: () => { this.error.set('Credential not found.'); this.loading.set(false); }
    });
  }
}
