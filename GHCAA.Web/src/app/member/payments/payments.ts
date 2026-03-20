import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService, PaymentRecord, MembershipDue } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { PaymentMethodSelectorComponent } from '../../common/payment-method-selector/payment-method-selector.component';
import {
    FINANCIAL_CATEGORY_OPTIONS,
    getFinancialCategoryLabel,
    getPaymentStatusClass,
    getPaymentStatusLabel
} from '../../core/constants/app.constants';
import { PaymentConfig } from '../../core/services/payment-config.service';

@Component({
    selector: 'app-payments',
    standalone: true,
    imports: [CommonModule, FormsModule, PaymentMethodSelectorComponent],
    templateUrl: './payments.html',
    styleUrl: './payments.scss'
})
export class Payments implements OnInit {
    private financialService = inject(FinancialService);
    private notify = inject(NotificationService);

    history = signal<PaymentRecord[]>([]);
    dues = signal<MembershipDue[]>([]);
    loading = signal(true);
    showPayModal = signal(false);
    categoryOptions = FINANCIAL_CATEGORY_OPTIONS;

    paymentForm = {
        amount: 0,
        transactionId: '',
        notes: '',
        paymentMethod: 'ManualReceipt',
        financialCategory: 'MembershipFee'
    };

    selectedPaymentMethod = signal<PaymentConfig | null>(null);
    selectedReceiptFile: File | null = null;

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);
        // ForkJoin would be better, but serial is fine for now
        this.financialService.getMyDues().subscribe(dues => {
            this.dues.set(dues);
            this.financialService.getMyHistory().subscribe(history => {
                this.history.set(history);
                this.loading.set(false);
            });
        });
    }

    openPayModal(due?: MembershipDue) {
        this.paymentForm = {
            amount: due ? due.amount : 0,
            transactionId: '',
            notes: due ? `Annual Dues for ${due.year}` : '',
            paymentMethod: 'ManualReceipt',
            financialCategory: due ? 'MembershipFee' : 'Donation'
        };
        this.showPayModal.set(true);
    }

    onPaymentMethodSelected(method: PaymentConfig) {
        this.selectedPaymentMethod.set(method);
        this.paymentForm.paymentMethod = method.method;
    }

    onReceiptSelected(event: any) {
        const file = event.target.files[0];
        if (file) this.selectedReceiptFile = file;
    }

    submitPayment() {
        if (!this.paymentForm.transactionId && this.selectedPaymentMethod()?.requiresReference) return;
        this.financialService.recordPayment(this.paymentForm).subscribe({
            next: () => {
                this.notify.success('Payment information submitted correctly. Status will be updated after verification.');
                this.showPayModal.set(false);
                this.selectedPaymentMethod.set(null);
                this.selectedReceiptFile = null;
                this.loadData();
            },
            error: () => this.notify.error('Failed to submit payment.')
        });
    }

    getStatusClass(status: any): string {
        return getPaymentStatusClass(status);
    }

    getStatusLabel(status: any): string {
        return getPaymentStatusLabel(status);
    }

    getCategoryLabel(val: any): string {
        return getFinancialCategoryLabel(val);
    }
}


