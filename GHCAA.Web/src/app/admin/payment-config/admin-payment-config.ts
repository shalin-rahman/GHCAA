import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PaymentConfigService, PaymentConfig } from '../../core/services/payment-config.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

@Component({
  selector: 'app-admin-payment-config',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageHeaderComponent, ImgFallbackDirective, LogoSpinnerComponent],
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

  // PaymentMethod enum names (see Domain/Enums.cs). Drives the create-form method dropdown so
  // any method — including CashOnHand — can be created, not just ManualReceipt (29G.3).
  methodOptions = ['ManualReceipt', 'BKash', 'Nagad', 'Rocket', 'CreditCard', 'BankTransfer', 'CashOnHand'];

  form = this.fb.group({
    method: ['ManualReceipt', Validators.required],
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

  openCreateForm() {
    this.editingId.set(null);
    this.form.reset({
      method: 'ManualReceipt',
      gateway: 'None',
      sortOrder: 0,
      requiresReceipt: true,
      requiresReference: true,
      isEnabled: true
    });
    this.showForm.set(true);
  }

  openEditForm(config: any) {
    this.editingId.set(config.id);
    this.form.patchValue(config);
    this.showForm.set(true);
  }

  submitForm() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.notify.error('Please correct the validation errors before saving.');
      return;
    }

    this.submitting.set(true);
    const id = this.editingId();
    const data = this.form.value;

    const req = id
        ? this.paymentService.updateConfig(id, data)
        : this.paymentService.createConfig(data); // method now comes from the form dropdown

    req.subscribe({
      next: () => {
        this.notify.success(id ? 'Payment configuration updated' : 'New payment method created');
        this.showForm.set(false);
        this.submitting.set(false);
        this.loadConfigs();
      },
      error: (err) => {
        this.notify.error(err.error?.message || 'Failed to save configuration');
        this.submitting.set(false);
      }
    });
  }

  getIconPath(icon: string): string {
    if (!icon) return '';
    if (icon.includes('/') || icon.includes('.')) return icon;
    
    const term = icon.toLowerCase().trim();
    if (term === 'bkash') return '/assets/images/bkash.png';
    if (term === 'nagad') return '/assets/images/nagad.png';
    if (term === 'rocket') return '/assets/images/rocket.png';
    if (term === 'sslcommerz') return '/assets/images/sslcommerz.png';
    if (term === 'visa') return '/assets/images/visa.png';
    if (term === 'master' || term === 'mastercard') return '/assets/images/master.png';
    if (term === 'card') return '/assets/images/card.png';
    if (term === 'visa-master') return '/assets/images/visa-master.png';
    
    return '';
  }

  isImageIcon(icon: string): boolean {
    if (!icon) return false;
    return icon.includes('/') || icon.includes('.') || 
           ['bkash', 'nagad', 'rocket', 'sslcommerz', 'visa', 'master', 'mastercard', 'card', 'visa-master'].includes(icon.toLowerCase().trim());
  }
}


