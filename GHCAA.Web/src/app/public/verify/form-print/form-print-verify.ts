import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ElectionsService } from '../../../core/services/elections.service';
import { ELECTION_PHASE_LABELS, ElectionFormPrintVerifyDto } from '../../../core/models/election.models';
import { AppDatePipe } from '../../../core/pipes/app-date.pipe';
import { utcToLocal } from '../../../core/utils/date.util';

const PRINT_ID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

// 37.14t. The page the QR code on a filled election form opens. It shows the print record,
// so a reader can compare the hash with the paper. It cannot prove the paper itself.
@Component({
  selector: 'app-form-print-verify',
  standalone: true,
  imports: [CommonModule, AppDatePipe],
  templateUrl: './form-print-verify.html',
  styleUrl: '../verify.scss'
})
export class FormPrintVerify implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(ElectionsService);
  readonly phaseLabels = ELECTION_PHASE_LABELS;
  readonly utcToLocal = utcToLocal;
  result = signal<ElectionFormPrintVerifyDto | null>(null);
  loading = signal(false);
  error = signal('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    if (!PRINT_ID.test(id)) {
      this.error.set('This link is not a valid print reference.');
      return;
    }
    this.loading.set(true);
    this.service.verifyPrint(id).subscribe({
      next: value => { this.result.set(value); this.loading.set(false); },
      error: () => { this.error.set('No print record matches this reference.'); this.loading.set(false); }
    });
  }
}
