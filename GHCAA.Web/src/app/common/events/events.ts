import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent, EventRegistration } from '../../core/models/business.models';
import { AuthService } from '../../core/services/auth.service';
import { PaymentMethodSelectorComponent } from '../../common/payment-method-selector/payment-method-selector.component';
import { PaymentConfig, PaymentConfigService } from '../../core/services/payment-config.service';
import { GatewaysService, PaymentGateway } from '../../core/services/gateways.service';
import { ActivatedRoute } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PaymentMethodSelectorComponent],
  templateUrl: './events.html',
  styleUrl: './events.scss'
})
export class Events implements OnInit {
  private eventsService = inject(EventsService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private gatewaysService = inject(GatewaysService);
  private notify = inject(NotificationService);

  events = signal<AlumniEvent[]>([]);
  activeTab = signal<'upcoming' | 'my-registrations'>('upcoming');
  myRegistrations = signal<EventRegistration[]>([]);

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

  regForm = this.fb.group({
    paymentReference: ['', [Validators.required, Validators.minLength(4)]],
    guestName: [''],
    guestEmail: ['', [Validators.email]],
    guestMobile: [''],
    contributionAmount: [null as number | null]
  });

  ngOnInit() {
    this.loadEvents();
    this.loadMyRegistrations();
  }

  loadEvents() {
    this.eventsService.getEvents().subscribe({
      next: data => {
        this.events.set(data);
        
        // Handle deep-link to auto-open a specific event registration
        // Check both query param (legacy) and route param (new /events/:id context)
        const targetEventId = this.route.snapshot.paramMap.get('id') || this.route.snapshot.queryParamMap.get('eventId');
        
        if (targetEventId) {
            const ev = data.find(e => e.id.toString() === targetEventId);
            if (ev) {
                // Slight delay ensures the UI has fully transitioned before opening the modal
                setTimeout(() => this.openRegisterModal(ev), 150);
            }
        }
      },
      error: () => this.events.set([])
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
    if (this.isGuest() && !ev.allowNonMembers) {
      this.notify.warning('This event is for members only. Please log in to register.');
      window.location.href = '/login';
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

  onPaymentMethodSelected(method: PaymentConfig) {
    this.selectedPaymentMethod.set(method);
    if (method.requiresReference) {
      this.regForm.get('paymentReference')?.setValidators([Validators.required, Validators.minLength(4)]);
    } else {
      this.regForm.get('paymentReference')?.clearValidators();
    }
    this.regForm.get('paymentReference')?.updateValueAndValidity();
    this.regForm.updateValueAndValidity();
  }

  submitRegistration() {
    if (this.regForm.invalid) {
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
        if (!this.selectedPaymentMethod()?.requiresReference && !ref) {
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
          this.notify.success('Project participation received! Wait for registry approval.');
          this.isSubmitting.set(false);
          this.closeModal();
          this.loadMyRegistrations();
        }
      },
      error: (err) => {
        console.error(err);
        this.formError.set(err.error?.message || 'Registration failed. Please check your inputs.');
        this.isSubmitting.set(false);
      }
    });
  }

  private initiateGateway(ev: AlumniEvent, ref: string) {
    const gateway = this.selectedPaymentMethod()?.method === 'SSLCommerz' ? PaymentGateway.SSLCommerz : PaymentGateway.Bkash;
    const amountToCharge = ev.registrationFee || this.regForm.value.contributionAmount || 0;

    this.gatewaysService.initiatePayment({
      amount: amountToCharge,
      gateway: gateway,
      reference: `EVT-REG-${ref}`,
      baseUrl: window.location.origin
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
    if (!ev.registrationDeadline) return true;
    return new Date(ev.registrationDeadline) > new Date();
  }

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
    const start = new Date(ev.date).toISOString().replace(/-|:|\.\d+/g, '');
    const end = new Date(new Date(ev.date).getTime() + 7200000).toISOString().replace(/-|:|\.\d+/g, ''); // 2h default
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


