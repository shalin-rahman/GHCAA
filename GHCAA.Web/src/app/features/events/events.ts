import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService, AlumniEvent, EventRegistration } from '../../core/services/events.service';
import { AuthService } from '../../core/services/auth.service';
import { PaymentMethodSelectorComponent } from '../../shared/payment-method-selector/payment-method-selector.component';
import { PaymentConfig } from '../../core/services/payment-config.service';
import { ActivatedRoute } from '@angular/router';

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

  regForm = this.fb.group({
    paymentReference: ['', [Validators.required, Validators.minLength(4)]],
    guestName: [''],
    guestEmail: ['', [Validators.email]],
    guestMobile: ['']
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
        const targetEventId = this.route.snapshot.queryParamMap.get('eventId');
        if (targetEventId) {
            const ev = data.find(e => e.id.toString() === targetEventId);
            if (ev) {
                // Slight delay ensures the UI has fully transitioned before opening the modal
                setTimeout(() => this.openRegisterModal(ev), 100);
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
      alert('This event is for members only. Please log in or apply for membership to register.');
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
    this.regForm.updateValueAndValidity();

    this.selectedEvent.set(ev);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.regForm.reset();
    this.selectedFile = null;
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
    if (this.regForm.invalid || !this.selectedEvent()) return;

    this.isSubmitting.set(true);
    const ev = this.selectedEvent()!;
    const isGuest = !this.auth.isAuthenticated();
    
    let ref = this.regForm.value.paymentReference || '';
    if (!this.selectedPaymentMethod()?.requiresReference && !ref) {
        ref = `NA-${Date.now().toString().slice(-6)}`;
    }

    const registrationDto: any = {
      eventId: ev.id,
      paymentReference: ref,
      paymentMethod: this.selectedPaymentMethod()?.method || 'ManualReceipt',
      isNonMember: isGuest,
      receiptFile: this.selectedFile || undefined
    };

    if (isGuest) {
      registrationDto.guestName = this.regForm.value.guestName!;
      registrationDto.guestEmail = this.regForm.value.guestEmail!;
      registrationDto.guestMobile = this.regForm.value.guestMobile!;
    }

    this.eventsService.registerForEvent(registrationDto).subscribe({
      next: () => {
        alert('Registration submitted successfully! Wait for admin approval.');
        this.isSubmitting.set(false);
        this.closeModal();
        this.loadMyRegistrations();
      },
      error: (err) => {
        console.error(err);
        alert(err.error?.message || 'Registration failed. Please check your inputs.');
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
        alert('Could not load invitation data.');
      }
    });
  }

  doPrint() {
    window.print();
  }

  closeInvitation() {
    this.showInvitation.set(false);
    this.invitationData.set(null);
  }
}
