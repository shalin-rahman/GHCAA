import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { FinancialService, PendingPayment } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { PAYMENT_REJECT_REASON } from '../../core/constants/app.constants';
import { serverErrorMessage } from '../../core/utils/http.util';
import { formatCurrencyAmount } from '../../core/utils/currency.util';
import { OrgConfigService } from '../../core/services/org-config.service';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { AppCurrencyPipe } from '../../core/pipes/app-currency.pipe';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

// Whitespace and control or format characters do not count. A little stricter than the API, which
// still counts zero-width characters.
export function visibleLength(text: string): number {
  return text.trim().replace(/[\s\p{C}]/gu, '').length;
}

const PROOF_URL_LIFETIME_MS = 30_000;

@Component({
  selector: 'app-payments-pending',
  standalone: true,
  imports: [CommonModule, FormsModule, AppDatePipe, AppCurrencyPipe, LoadingPanelComponent, PageHeaderComponent, ModalHeaderComponent],
  templateUrl: './payments-pending.html',
  styleUrl: './payments-pending.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentsPending implements OnInit {
  private financial = inject(FinancialService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);
  private orgConfig = inject(OrgConfigService);

  readonly reasonMax = PAYMENT_REJECT_REASON.MAX;

  payments = signal<PendingPayment[]>([]);
  loading = signal(true);
  busyId = signal<number | null>(null);
  rejecting = signal<PendingPayment | null>(null);
  rejectReason = signal('');
  rejectError = signal('');

  canReject = computed(() => {
    const reason = this.rejectReason();
    return visibleLength(reason) >= PAYMENT_REJECT_REASON.MIN_VISIBLE
      && reason.trim().length <= PAYMENT_REJECT_REASON.MAX
      && this.busyId() === null;
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.financial.getPendingPayments().subscribe({
      next: list => {
        this.payments.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // The tab opens inside the click so popup blockers allow it; the proof fills it once loaded.
  openProof(p: PendingPayment) {
    if (!p.proofPath) return;
    const tab = window.open('', '_blank');
    if (!tab) {
      this.notify.error('The browser blocked the new tab. Allow pop-ups for this site to view the proof.');
      return;
    }
    tab.opener = null;
    this.financial.getProof(p.proofPath).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        tab.location.href = url;
        setTimeout(() => URL.revokeObjectURL(url), PROOF_URL_LIFETIME_MS);
      },
      error: () => {
        tab.close();
        this.notify.error('Could not load the proof. Please try again.');
      },
    });
  }

  verify(p: PendingPayment) {
    const amount = formatCurrencyAmount(p.amount, this.orgConfig.config()?.currency);
    const reference = p.transactionReference ? ` (reference ${p.transactionReference})` : '';
    this.confirmDialog.confirm({
      title: 'Verify payment',
      message: `Mark the payment of ${amount} from ${p.memberName ?? 'this member'}${reference} as received?`,
      confirmLabel: 'Verify',
    }).subscribe(ok => {
      if (!ok) return;
      this.busyId.set(p.id);
      this.financial.verifyPayment(p.id).subscribe({
        next: res => {
          this.busyId.set(null);
          this.notify.success(res.dueId != null
            ? `Payment verified. Due #${res.dueId} is now paid.`
            : 'Payment verified. No due settled: no single open due matched this member and amount.');
          this.load();
        },
        error: (err: HttpErrorResponse) => this.onReviewError(err, 'Could not verify the payment.'),
      });
    });
  }

  openReject(p: PendingPayment) {
    this.rejecting.set(p);
    this.rejectReason.set('');
    this.rejectError.set('');
  }

  closeReject() {
    this.rejecting.set(null);
  }

  reject() {
    const p = this.rejecting();
    if (!p || !this.canReject()) return;
    this.busyId.set(p.id);
    this.rejectError.set('');
    this.financial.rejectPayment(p.id, this.rejectReason().trim()).subscribe({
      next: () => {
        this.busyId.set(null);
        this.rejecting.set(null);
        this.notify.success('Payment rejected. The member sees the reason on their payments page.');
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        // Only a gone or already reviewed payment closes the dialog; otherwise the typed reason stays.
        if (err.status === 409 || err.status === 404) {
          this.rejecting.set(null);
          this.onReviewError(err, 'Could not reject the payment.');
          return;
        }
        this.busyId.set(null);
        this.rejectError.set(serverErrorMessage(err, err.status === 400
          ? 'The reason was not accepted.'
          : 'Could not reject the payment. Please try again.'));
      },
    });
  }

  // 409 means another admin reviewed it first, so the list is stale.
  private onReviewError(err: HttpErrorResponse, fallback: string) {
    this.busyId.set(null);
    this.notify.error(serverErrorMessage(err, fallback));
    if (err.status === 409 || err.status === 404) this.load();
  }
}
