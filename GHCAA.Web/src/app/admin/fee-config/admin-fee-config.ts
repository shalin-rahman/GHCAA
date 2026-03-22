import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FinancialService } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { FINANCIAL_CATEGORY_OPTIONS, MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';

@Component({
  selector: 'app-admin-fee-config',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-fee-config.html',
  styleUrl: './admin-fee-config.scss'
})
export class AdminFeeConfig implements OnInit {
  private financialService = inject(FinancialService);
  private notify = inject(NotificationService);
  private fb = inject(FormBuilder);

  configs = signal<any[]>([]);
  loading = signal(true);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  submitting = signal(false);

  feeCategories = FINANCIAL_CATEGORY_OPTIONS;
  membershipTypes = MEMBERSHIP_TYPE_OPTIONS;

  form = this.fb.group({
    category: ['RegistrationFee', Validators.required],
    membershipType: ['General', Validators.required],
    amount: [0, [Validators.required, Validators.min(0)]],
    effectiveDate: ['', Validators.required],
    effectiveTo: [null],
    isActive: [true]
  });

  ngOnInit() {
    this.loadConfigs();
  }

  loadConfigs() {
    this.loading.set(true);
    this.financialService.getFeeConfigs().subscribe({
      next: (data) => {
        this.configs.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load fee configurations');
        this.loading.set(false);
      }
    });
  }

  openCreateForm() {
    this.editingId.set(null);
    this.form.reset({
      category: 'RegistrationFee',
      membershipType: 'General',
      amount: 0,
      effectiveDate: new Date().toISOString().split('T')[0],
      isActive: true
    });
    this.showForm.set(true);
  }

  openEditForm(config: any) {
    this.editingId.set(config.id);
    this.form.patchValue({
      ...config,
      effectiveDate: config.effectiveDate ? new Date(config.effectiveDate).toISOString().split('T')[0] : '',
      effectiveTo: config.effectiveTo ? new Date(config.effectiveTo).toISOString().split('T')[0] : null
    });
    this.showForm.set(true);
  }

  submitForm() {
    if (this.form.invalid) return;

    this.submitting.set(true);
    const id = this.editingId();
    const data = this.form.value;

    const req = id 
      ? this.financialService.updateFeeConfig({ ...data, id })
      : this.financialService.addFeeConfig(data);

    req.subscribe({
      next: () => {
        this.notify.success(id ? 'Configuration updated' : 'New configuration added');
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

  deleteConfig(id: number) {
    if (confirm('Are you sure you want to delete this configuration? This might affect existing member billing logic.')) {
        // Implementation for delete if backend supports it. For now, we can just deactivate.
        this.notify.info('Direct deletion disabled to preserve financial integrity. Please deactivate instead.');
    }
  }

  getCategoryLabel(val: string) {
    return this.feeCategories.find(c => c.value === val)?.label || val;
  }
}
