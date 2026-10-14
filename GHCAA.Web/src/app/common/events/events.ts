import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent, EventRegistration, PaymentGateway } from '../../core/models/business.models';
import { AuthService } from '../../core/services/auth.service';
import { PaymentPortalComponent } from '../../common/payment-portal/payment-portal.component';
import { PaymentConfig, PaymentConfigService } from '../../core/services/payment-config.service';
import { GatewaysService } from '../../core/services/gateways.service';
import { ActivatedRoute } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { FinancialService } from '../../core/services/financial.service';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { LazyImgDirective } from '../directives/lazy-img.directive';
import { ROUTES, LIST_PAGE_SIZE } from '../../core/constants/app.constants';
import { appendById } from '../../core/utils/paging.util';
import { Subscription } from 'rxjs';
import { getEventStatusMeta } from '../../core/utils/date.util';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ModalHeaderComponent } from '../modal-header/modal-header.component';
import { PageHeaderComponent } from '../page-header/page-header.component';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';
import { AppCurrencyPipe } from '../../core/pipes/app-currency.pipe';
import { LoadMoreComponent } from '../load-more/load-more';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, AppDatePipe, FormsModule, ReactiveFormsModule, PaymentPortalComponent, ImgFallbackDirective, LazyImgDirective, ModalHeaderComponent, PageHeaderComponent, LoadingPanelComponent, AppCurrencyPipe, LoadMoreComponent],
  templateUrl: './events.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './events.scss'
})
export class Events implements OnInit {
  private eventsService = inject(EventsService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private gatewaysService = inject(GatewaysService);
  orgConfig = inject(OrgConfigService);
  private notify = inject(NotificationService);
  private finService = inject(FinancialService);

  events = signal<AlumniEvent[]>([]);
  hasMore = signal(false);
  loadingMore = signal(false);
  private page = 1;
  private pageSub?: Subscription;
  activeTab = signal<'upcoming' | 'my-registrations'>('upcoming');
  myRegistrations = signal<EventRegistration[]>([]);

  // 30.26: holds a deep-linked event (from /events/:id or ?eventId=) until the auth service has
  // finished restoring the session (see AuthService.authChecked). Without this gate, a genuine
  // member landing here via a fresh page load could get misidentified as a guest — and bounced
  // to /login — because openRegisterModal()'s isGuest() check ran before the async /auth/me
  // session-restore call had resolved (a real race between two independent HTTP calls).
  private pendingDeepLinkEvent = signal<AlumniEvent | null>(null);
  private deepLinkEffect = effect(() => {
    const ev = this.pendingDeepLinkEvent();
    if (ev && this.auth.authChecked()) {
      this.pendingDeepLinkEvent.set(null);
      // Slight delay ensures the UI has fully transitioned before opening the modal
      setTimeout(() => this.openRegisterModal(ev), 150);
    }
  });

  // Modal & Form State
  showModal = signal<boolean>(false);
  showInvitation = signal<boolean>(false);
  selectedEvent = signal<AlumniEvent | null>(null);
  invitationData = signal<EventRegistration | null>(null);
  selectedFile: File | null = null;
  isSubmitting = signal<boolean>(false);
  selectedPaymentMethod = signal<PaymentConfig | null>(null);
  
  // Public Participants State
  participants = signal<any[]>([]);
  showParticipants = signal<boolean>(false);
  activeParticipantEventId = signal<number | null>(null);
  loadingParticipants = signal<boolean>(false);
  formError = signal<string | null>(null);
  saveMethodRequested = signal<boolean>(false);
  saveMethodLabel = signal<string>('');

  regForm = this.fb.group({
    paymentReference: ['', [Validators.required, Validators.minLength(4)]],
    guestName: [''],
    guestEmail: ['', [Validators.email]],
    guestMobile: [''],
    contributionAmount: [null as number | null]
  });

  constructor() {
    // Gated on authChecked() rather than a one-shot isAuthenticated() read in ngOnInit: the
    // /auth/me session restore is deferred (see AuthService), so a member's registrations would
    // otherwise never load on a fresh page — ngOnInit runs before the restore resolves and
    // nothing re-checks afterward. authChecked() is already true on construction whenever a
    // cached session was found, so the common case still loads immediately.
    this.auth.whenAuthenticated(() => this.loadMyRegistrations());
  }

  ngOnInit() {
    this.loadEvents();
  }

  loadEvents() {
    this.page = 1;
    // A next page still in flight belongs to the old list.
    this.pageSub?.unsubscribe();
    this.loadingMore.set(false);
    this.pageSub = this.eventsService.getEventsPage(1, LIST_PAGE_SIZE).subscribe({
      next: result => {
        this.events.set(result.items);
        this.hasMore.set(result.page < result.totalPages);

        // Handle deep-link to auto-open a specific event registration
        // Check both query param (legacy) and route param (new /events/:id context)
        this.resolveDeepLink(this.route.snapshot.paramMap?.get('id') || this.route.snapshot.queryParamMap.get('eventId'));
      },
      error: () => this.events.set([])
    });
  }

  loadNextPage() {
    if (!this.hasMore() || this.loadingMore()) return;
    this.loadingMore.set(true);
    this.pageSub?.unsubscribe();
    this.pageSub = this.eventsService.getEventsPage(this.page + 1, LIST_PAGE_SIZE).subscribe({
      next: result => {
        this.page = result.page;
        // A deep-linked event may already sit at the top of the list.
        this.events.update(list => appendById(list, result.items));
        this.hasMore.set(result.page < result.totalPages);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false)
    });
  }

  // The linked event may sit past the first page, so it is fetched on its own when not loaded.
  // It goes to the top of the list, because the registration form renders inside its row.
  // Only active events open, as before paging.
  private resolveDeepLink(targetEventId: string | null) {
    if (!targetEventId) return;
    const loaded = this.events().find(e => e.id.toString() === targetEventId);
    if (loaded) {
      // Defer to the effect above until auth state is confirmed (see 30.26 note).
      this.pendingDeepLinkEvent.set(loaded);
      return;
    }
    const id = Number(targetEventId);
    if (!Number.isInteger(id)) return;
    this.eventsService.getEventById(id).subscribe({
      next: ev => {
        if (!ev?.isActive) return;
        this.events.update(list => list.some(e => e.id === ev.id) ? list : [ev, ...list]);
        this.pendingDeepLinkEvent.set(ev);
      },
      error: () => {}
    });
  }

  loadMyRegistrations() {
    if (this.auth.isAuthenticated()) {
      this.eventsService.getMyRegistrations().subscribe({
        next: data => this.myRegistrations.set(data),
        error: () => this.myRegistrations.set([])
      });
    }
  }


  openRegisterModal(ev: AlumniEvent) {
    if (ev.requiresRegistration === false) {
      // Informational-only event — no registration flow should ever be offered.
      return;
    }
    if (this.isGuest() && !ev.allowNonMembers) {
      this.notify.warning('This event is for members only. Please log in to register.');
      window.location.href = ROUTES.LOGIN;
      return;
    }
    
    // Set dynamic validators if guest
    if (!this.auth.isAuthenticated()) {
      this.regForm.get('guestName')?.setValidators([Validators.required]);
      this.regForm.get('guestEmail')?.setValidators([Validators.required, Validators.email]);
      this.regForm.get('guestMobile')?.setValidators([Validators.required]);
    } else {
      this.regForm.get('guestName')?.clearValidators();
      this.regForm.get('guestEmail')?.clearValidators();
      this.regForm.get('guestMobile')?.clearValidators();
    }
    
    // Explicitly update validity state locally on controls to recalculate status
    this.regForm.get('guestName')?.updateValueAndValidity();
    this.regForm.get('guestEmail')?.updateValueAndValidity();
    this.regForm.get('guestMobile')?.updateValueAndValidity();

    if (!ev.requiresPayment) {
       this.regForm.get('paymentReference')?.clearValidators();
       this.regForm.get('paymentReference')?.updateValueAndValidity();
       this.regForm.get('contributionAmount')?.clearValidators();
    } else if (!ev.registrationFee) {
       this.regForm.get('contributionAmount')?.setValidators([Validators.required, Validators.min(10)]); // Min 10 BDT
    } else {
       this.regForm.get('contributionAmount')?.clearValidators();
    }
    
    this.regForm.get('contributionAmount')?.updateValueAndValidity();
    this.regForm.updateValueAndValidity();

    this.selectedEvent.set(ev);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.regForm.reset();
    this.selectedFile = null;
    this.formError.set(null);
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onPaymentMethodSelected(method: any) {
    this.selectedPaymentMethod.set(method);
    if (method.requiresReference) {
      this.regForm.get('paymentReference')?.setValidators([Validators.required, Validators.minLength(4)]);
    } else {
      this.regForm.get('paymentReference')?.clearValidators();
    }
    this.regForm.get('paymentReference')?.updateValueAndValidity();
    this.regForm.updateValueAndValidity();
  }

  onReferenceChange(ref: string) {
    this.regForm.get('paymentReference')?.setValue(ref);
  }

  onReceiptSelected(file: File) {
    this.selectedFile = file;
  }

  onSaveRequested(data: {save: boolean, label: string}) {
    this.saveMethodRequested.set(data.save);
    this.saveMethodLabel.set(data.label);
  }

  submitRegistration() {
    if (this.regForm.invalid || !this.selectedEvent()) {
      this.regForm.markAllAsTouched();
      this.formError.set('Please provide all required information marked in red.');
      return;
    }
    
    this.formError.set(null);
    
    const ev = this.selectedEvent();
    if (!ev) return;
    
    if (ev.requiresPayment && !this.selectedPaymentMethod()) {
      this.formError.set('Please select a payment method.');
      return;
    }

    this.isSubmitting.set(true);
    
    let ref = this.regForm.value.paymentReference || '';
    if (ev.requiresPayment) {
        if (this.selectedPaymentMethod()?.gateway !== 'None') {
            // Online Gateway uses a distinct verifiable reference
            ref = `EVT-REG-${Date.now().toString().slice(-8)}`;
        } else if (!this.selectedPaymentMethod()?.requiresReference && !ref) {
            ref = `NA-${Date.now().toString().slice(-6)}`;
        }
    } else {
        ref = 'FREE-ENTRY';
    }

    const registrationDto: any = {
      eventId: ev.id,
      paymentReference: ref,
      paymentMethod: this.selectedPaymentMethod()?.method || 'ManualReceipt',
      contributionAmount: this.regForm.value.contributionAmount || undefined,
      isNonMember: this.isGuest(),
      receiptFile: this.selectedFile || undefined
    };

    if (this.isGuest()) {
      registrationDto.guestName = this.regForm.value.guestName!;
      registrationDto.guestEmail = this.regForm.value.guestEmail!;
      registrationDto.guestMobile = this.regForm.value.guestMobile!;
    }

    this.eventsService.registerForEvent(registrationDto).subscribe({
      next: () => {
        if (this.selectedPaymentMethod()?.isOnline) {
          this.initiateGateway(ev, ref);
        } else {
          // If user requested to save this manual method for future
          if (this.saveMethodRequested() && this.selectedPaymentMethod()) {
              const m = this.selectedPaymentMethod()!;
              this.finService.addSavedMethod({
                  displayName: this.saveMethodLabel() || m.displayName,
                  method: m.method || m.displayName,
                  accountNumber: '' // Leave empty for manual hints or add logic
              }).subscribe();
          }

          this.notify.success('Project participation received! Wait for registry approval.');
          this.isSubmitting.set(false);
          this.closeModal();
          this.loadMyRegistrations();
        }
      },
      error: (err: any) => {
        console.error(err);
        this.formError.set(err.error?.message || 'Registration failed. Please check your inputs.');
        this.isSubmitting.set(false);
      }
    });
  }

  public initiateGateway(ev: AlumniEvent, ref: string) {
    const gatewayStr = this.selectedPaymentMethod()?.gateway;
    const gateway = (gatewayStr && PaymentGateway[gatewayStr as keyof typeof PaymentGateway] !== undefined) 
      ? PaymentGateway[gatewayStr as keyof typeof PaymentGateway] 
      : PaymentGateway.None;
      
    const amountToCharge = ev.registrationFee || this.regForm.value.contributionAmount || 0;

    const user = this.auth.currentUser();
    this.gatewaysService.initiatePayment({
      amount: amountToCharge,
      gateway: gateway,
      reference: ref, // Now contains prefix from earlier logic
      baseUrl: window.location.origin,
      customerName: user?.fullName || this.regForm.value.guestName || 'Guest',
      customerEmail: user?.email || this.regForm.value.guestEmail || '',
      customerPhone: user?.mobileNo || this.regForm.value.guestMobile || ''
    }).subscribe({
      next: (res) => {
        if (res.success && res.gatewayUrl) {
          window.location.href = res.gatewayUrl;
        } else {
          this.formError.set('Gateway initiation failed: ' + res.message);
          this.isSubmitting.set(false);
        }
      },
      error: () => {
        this.formError.set('Could not initiate online payment. Please try manual receipt upload.');
        this.isSubmitting.set(false);
      }
    });
  }

  isGuest(): boolean {
    return !this.auth.isAuthenticated();
  }

  isRegistrationOpen(ev: AlumniEvent): boolean {
    if (!ev.isActive) return false;
    const now = new Date();
    // 54.6: registrationEndDate is optional — without this, an event with no explicit
    // registration deadline stayed "open" here indefinitely even long after its own endDate.
    if (ev.endDate && new Date(ev.endDate) < now) return false;
    if (ev.registrationStartDate && new Date(ev.registrationStartDate) > now) return false;
    if (ev.registrationEndDate && new Date(ev.registrationEndDate) < now) return false;
    return true;
  }

  isEventEnded(ev: AlumniEvent): boolean {
    return !!ev.endDate && new Date(ev.endDate) < new Date();
  }

  // Date-computed lifecycle badge (Upcoming/Ongoing/Ended) shown on each event card — the event's
  // visible status was previously implied only by the register button's Open/Closed state.
  getStatusMeta = getEventStatusMeta;

  askAdmin(ev: AlumniEvent) {
    const message = `I have a question about the event: ${ev.title}`;
    // Navigate to chat or open a contact modal
    // For now, we'll use a mailto or an alert
    window.location.href = `mailto:admin@ghcaa.org?subject=Query on ${ev.title}&body=${message}`;
  }

  printInvitation(regId: number) {
    this.eventsService.getRegistrationForInvitation(regId).subscribe({
      next: (data) => {
        this.invitationData.set(data);
        this.showInvitation.set(true);
        // Small delay to let the modal render before printing if needed, 
        // or just let the user click print in the modal.
      },
      error: (err) => {
        this.notify.error('Could not load invitation data.');
      }
    });
  }

  doPrint() {
    window.print();
  }

  getCalendarLink(ev: AlumniEvent): string {
    const start = new Date(ev.startDate).toISOString().replace(/-|:|\.\d+/g, '');
    const end = ev.endDate
      ? new Date(ev.endDate).toISOString().replace(/-|:|\.\d+/g, '')
      : new Date(new Date(ev.startDate).getTime() + 7200000).toISOString().replace(/-|:|\.\d+/g, '');
    return `https://www.google.com/calendar/render?action=TEMPLATE&text=${encodeURIComponent(ev.title)}&dates=${start}/${end}&details=${encodeURIComponent(ev.description)}&location=${encodeURIComponent(ev.location)}`;
  }

  closeInvitation() {
    this.showInvitation.set(false);
    this.invitationData.set(null);
  }

  toggleParticipants(eventId: number) {
    if (this.activeParticipantEventId() === eventId && this.showParticipants()) {
      this.showParticipants.set(false);
      this.activeParticipantEventId.set(null);
      return;
    }

    this.activeParticipantEventId.set(eventId);
    this.loadingParticipants.set(true);
    this.eventsService.getPublicParticipants(eventId).subscribe({
      next: data => {
        this.participants.set(data);
        this.showParticipants.set(true);
        this.loadingParticipants.set(false);
      },
      error: () => {
        this.participants.set([]);
        this.loadingParticipants.set(false);
      }
    });
  }
}
