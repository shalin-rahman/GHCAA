import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PaymentConfigService, PaymentConfig } from '../../core/services/payment-config.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-payment-config',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-payment-config.html',
  styleUrl: './admin-payment-config.scss'
})
export class AdminPaymentConfig implements OnInit {
  private paymentService = inject(PaymentConfigService);
  private notify = inject(NotificationService);
  private fb = inject(FormBuilder);

  configs = signal<any[]>([]);
  loading = signal(true);
  
  showForm = signal(false);
  editingId = signal<number | null>(null);
  submitting = signal(false);

  form = this.fb.group({
    displayName: ['', Validators.required],
    description: [''],
    icon: [''],
    walletNumber: [''],
    accountHolderName: [''],
    bankName: [''],
    branchName: [''],
    accountNumber: [''],
    routingNumber: [''],
    instructions: [''],
    gateway: ['None'],
    gatewayPublicKey: [''],
    gatewaySecretKey: [''],
    gatewayCallbackUrl: [''],
    sortOrder: [0, Validators.required],
    requiresReceipt: [true],
    requiresReference: [true],
    isEnabled: [true]
  });

  ngOnInit() {
    this.loadConfigs();
  }

  loadConfigs() {
    this.loading.set(true);
    this.paymentService.getAllConfigs().subscribe({
      next: (data) => {
        this.configs.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  seedDefaults() {
    if (confirm('Are you sure you want to seed default payment methods? This should only be done if none exist.')) {
      this.paymentService.seedDefaults().subscribe({
        next: () => {
          this.notify.success('Default payment methods created successfully');
          this.loadConfigs();
        },
        error: (err) => this.notify.error(err.error || 'Failed to seed defaults')
      });
    }
  }

  toggleStatus(id: number) {
    this.paymentService.toggleConfig(id).subscribe({
      next: () => {
        this.notify.success('Status updated');
        this.loadConfigs();
      },
      error: () => this.notify.error('Failed to update status')
    });
  }
  
  deleteConfig(id: number) {
    if (confirm('Are you sure you want to delete this payment method?')) {
      this.paymentService.deleteConfig(id).subscribe({
        next: () => {
          this.notify.success('Payment method deleted');
          this.loadConfigs();
        },
        error: () => this.notify.error('Failed to delete')
      });
    }
  }

  openEditForm(config: any) {
    this.editingId.set(config.id);
    this.form.patchValue(config);
    this.showForm.set(true);
  }

  submitForm() {
    if (this.form.invalid) return;

    this.submitting.set(true);
    const id = this.editingId();
    const data = this.form.value;

    const req = id 
        ? this.paymentService.updateConfig(id, data)
        : null; // Creating new from UI might require the backend "Method" enum, limiting to edit for now or handling it. Let's just do update since seed covers creation in this app.

    if (req) {
      req.subscribe({
        next: () => {
          this.notify.success('Payment configuration updated');
          this.showForm.set(false);
          this.submitting.set(false);
          this.loadConfigs();
        },
        error: () => {
          this.notify.error('Failed to update configuration');
          this.submitting.set(false);
        }
      });
    }
  }
}
