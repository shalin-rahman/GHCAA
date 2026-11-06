import { Component, ChangeDetectionStrategy, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, catchError, of, switchMap } from 'rxjs';
import { AppDatePipe } from '../../../core/pipes/app-date.pipe';
import { utcToLocal } from '../../../core/utils/date.util';
import { CREDENTIAL_REASON_MAX_LENGTH } from '../../../core/constants/app.constants';
import { NotificationService } from '../../../core/services/notification.service';
import {
  CredentialType,
  CredentialVerificationService,
  IssuedCredential
} from '../../../core/services/credential-verification.service';

export type CredentialAction = 'revoke' | 'reissue';

const TYPE_LABELS: Record<CredentialType, string> = {
  MembershipCertificate: 'Certificate',
  IdCard: 'ID card',
  ElectionDocument: 'Election document'
};

// 84.68: the codes printed on a member's ID cards and certificates, with Revoke and Reissue.
@Component({
  selector: 'app-member-credentials',
  standalone: true,
  imports: [FormsModule, AppDatePipe],
  templateUrl: './member-credentials.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberCredentialsComponent implements OnChanges {
  private credentials = inject(CredentialVerificationService);
  private notify = inject(NotificationService);

  @Input({ required: true }) memberId!: number;

  readonly reasonMaxLength = CREDENTIAL_REASON_MAX_LENGTH;
  items = signal<IssuedCredential[]>([]);
  loading = signal(false);
  // Only one row shows the reason box at a time.
  pending = signal<{ shortCode: string; action: CredentialAction } | null>(null);
  reason = signal('');
  busy = signal(false);

  // switchMap drops a slower reply for the previous member when the admin opens another one.
  private readonly reload$ = new Subject<number>();

  constructor() {
    this.reload$.pipe(
      switchMap(id => this.credentials.getMemberCredentials(id).pipe(catchError(() => of([] as IssuedCredential[])))),
      takeUntilDestroyed()
    ).subscribe(list => { this.items.set(list); this.loading.set(false); });
  }

  ngOnChanges(): void {
    this.pending.set(null);
    this.load();
  }

  typeLabel(type: CredentialType): string {
    return TYPE_LABELS[type] ?? type;
  }

  isExpired(item: IssuedCredential): boolean {
    const expires = utcToLocal(item.expiresOn);
    return !!expires && expires.getTime() < Date.now();
  }

  start(shortCode: string, action: CredentialAction): void {
    this.reason.set('');
    this.pending.set({ shortCode, action });
  }

  cancel(): void {
    this.pending.set(null);
  }

  submit(): void {
    const pending = this.pending();
    const reason = this.reason().trim();
    if (!pending || !reason || this.busy()) return;

    this.busy.set(true);
    const request: Observable<IssuedCredential | void> = pending.action === 'revoke'
      ? this.credentials.revoke(pending.shortCode, reason)
      : this.credentials.reissue(pending.shortCode, reason);

    request.subscribe({
      next: result => {
        this.busy.set(false);
        this.pending.set(null);
        this.notify.success(result
          ? `Code ${pending.shortCode} replaced by ${result.shortCode}.`
          : `Code ${pending.shortCode} revoked.`);
        this.load();
      },
      // The global interceptor already shows the server message.
      error: () => this.busy.set(false)
    });
  }

  private load(): void {
    if (!this.memberId) return;
    this.loading.set(true);
    this.reload$.next(this.memberId);
  }
}
