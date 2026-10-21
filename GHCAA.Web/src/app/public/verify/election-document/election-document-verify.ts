import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ElectionsService } from '../../../core/services/elections.service';
import { ELECTION_PHASE_LABELS, ElectionDocumentVerifyDto } from '../../../core/models/election.models';

// 37.14n. The page the QR code on a printed election form opens. It confirms the
// election and the form exist. It cannot vouch for the paper, only for the record.
@Component({
  selector: 'app-election-document-verify',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './election-document-verify.html',
  styleUrl: '../verify.scss'
})
export class ElectionDocumentVerify implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(ElectionsService);
  readonly phaseLabels = ELECTION_PHASE_LABELS;
  result = signal<ElectionDocumentVerifyDto | null>(null);
  loading = signal(false);
  error = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    const code = this.route.snapshot.paramMap.get('code') ?? '';
    if (!Number.isInteger(id) || id <= 0 || !code.trim()) {
      this.error.set('This link is not a valid election form reference.');
      return;
    }
    this.loading.set(true);
    this.service.verifyDocument(id, code).subscribe({
      next: value => { this.result.set(value); this.loading.set(false); },
      error: () => { this.error.set('No election form matches this reference.'); this.loading.set(false); }
    });
  }
}
