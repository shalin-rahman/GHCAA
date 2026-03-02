import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EventsService, AlumniEvent, EventRegistration } from '../../core/services/events.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './events.html',
  styleUrl: './events.scss'
})
export class Events implements OnInit {
  private eventsService = inject(EventsService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);

  events = signal<AlumniEvent[]>([]);
  activeTab = signal<'upcoming' | 'my-registrations' | 'admin'>('upcoming');
  myRegistrations = signal<EventRegistration[]>([]);
  adminRegistrations = signal<EventRegistration[]>([]);

  // Modal & Form State
  showModal = signal<boolean>(false);
  showCreateEvent = signal<boolean>(false);
  selectedEvent = signal<AlumniEvent | null>(null);
  selectedFile: File | null = null;
  isSubmitting = signal<boolean>(false);

  isAdmin = computed(() => this.auth.currentUser()?.role === 'SuperAdmin' || this.auth.currentUser()?.role === 'Admin');

  regForm = this.fb.group({
    paymentReference: ['', [Validators.required, Validators.minLength(4)]]
  });

  eventForm = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    date: ['', Validators.required],
    location: ['', Validators.required],
    registrationFee: [0],
    registrationDeadline: [''],
    adminNote: [''],
    isActive: [true]
  });

  ngOnInit() {
    this.loadEvents();
    this.loadMyRegistrations();
    if (this.isAdmin()) {
      this.loadAdminRegistrations();
    }
  }

  loadEvents() {
    this.eventsService.getEvents().subscribe({
      next: data => this.events.set(data),
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

  loadAdminRegistrations() {
    this.eventsService.getAllRegistrations().subscribe({
      next: data => this.adminRegistrations.set(data),
      error: () => this.adminRegistrations.set([])
    });
  }

  openRegisterModal(ev: AlumniEvent) {
    if (!this.auth.isAuthenticated()) {
      alert('Please login to register for events.');
      return;
    }
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

  submitRegistration() {
    if (this.regForm.invalid || !this.selectedEvent()) return;

    this.isSubmitting.set(true);
    const evId = this.selectedEvent()!.id;
    const ref = this.regForm.value.paymentReference!;

    this.eventsService.registerForEvent(evId, ref, this.selectedFile || undefined).subscribe({
      next: () => {
        alert('Registration submitted successfully! Wait for admin approval.');
        this.isSubmitting.set(false);
        this.closeModal();
        this.loadMyRegistrations();
        if (this.isAdmin()) this.loadAdminRegistrations();
      },
      error: (err) => {
        console.error(err);
        alert('Registration failed. Please try again.');
        this.isSubmitting.set(false);
      }
    });
  }

  submitEvent() {
    if (this.eventForm.invalid) return;

    const evData = this.eventForm.value as Partial<AlumniEvent>;
    this.eventsService.createEvent(evData).subscribe({
      next: () => {
        alert('Event created successfully!');
        this.showCreateEvent.set(false);
        this.eventForm.reset({ isActive: true });
        this.loadEvents();
      },
      error: (err) => {
        console.error(err);
        alert('Failed to create event.');
      }
    });
  }

  approveReg(regId: number, approve: boolean) {
    const action = approve ? 'approve' : 'reject';
    if (confirm(`Are you sure you want to ${action} this registration?`)) {
      this.eventsService.approveRegistration(regId, approve).subscribe(() => {
        this.loadAdminRegistrations();
        this.loadMyRegistrations(); // Refresh if current user is the one approved
      });
    }
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
}
