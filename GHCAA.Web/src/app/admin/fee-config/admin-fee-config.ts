import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FinancialService } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { FINANCIAL_CATEGORY_OPTIONS, MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';

@Component({
  selector: 'app-admin-fee-config',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, LogoSpinnerComponent, PageHeaderComponent],
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

  formatDateToDMY(d: any) {
    return toDisplayDate(d);
  }

  feeCategories = FINANCIAL_CATEGORY_OPTIONS;
  membershipTypes = MEMBERSHIP_TYPE_OPTIONS;

  static configDatesValidator(group: import('@angular/forms').AbstractControl): import('@angular/forms').ValidationErrors | null {
    const start = group.get('effectiveDate')?.value;
    const end = group.get('effectiveTo')?.value;
    if (start && end && new Date(end) <= new Date(start)) {
        return { endBeforeStart: "'Effective To' date must be after 'Effective From' date." };
    }
    return null;
  }

  form = this.fb.group({
    category: ['RegistrationFee', Validators.required],
    membershipType: ['General', Validators.required],
    amount: [0, [Validators.required, Validators.min(0)]],
    effectiveDate: ['', Validators.required],
    effectiveTo: [null],
    isActive: [true],
    description: ['']
  }, { validators: AdminFeeConfig.configDatesValidator });

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
      effectiveDate: this.formatDateToDMY(new Date()),
      isActive: true
    });
    this.showForm.set(true);
  }

  openEditForm(config: any) {
    this.editingId.set(config.id);
    this.form.patchValue({
      ...config,
      effectiveDate: this.formatDateToDMY(config.effectiveDate),
      effectiveTo: this.formatDateToDMY(config.effectiveTo),
      description: config.description || ''
    });
    this.showForm.set(true);
  }

  submitForm() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      if (this.form.errors?.['endBeforeStart']) {
        this.notify.error(this.form.errors['endBeforeStart']);
      } else {
        this.notify.error('Please fix the errors in the form.');
      }
      return;
    }

    this.submitting.set(true);
    const id = this.editingId();
    const data = {
      ...this.form.value,
      effectiveDate: toWireDate(this.form.value.effectiveDate),
      effectiveTo: toWireDate(this.form.value.effectiveTo)
    };

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
