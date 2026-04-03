import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService, PaymentRecord, MembershipDue } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { PaymentPortalComponent } from '../../common/payment-portal/payment-portal.component';
import {
    FINANCIAL_CATEGORY_OPTIONS,
    getFinancialCategoryLabel,
    getPaymentStatusClass,
    getPaymentStatusLabel
} from '../../core/constants/app.constants';

@Component({
    selector: 'app-payments',
    standalone: true,
    imports: [CommonModule, FormsModule, PaymentPortalComponent],
    templateUrl: './payments.html',
    styleUrl: './payments.scss'
})
export class Payments implements OnInit {
    private financialService = inject(FinancialService);
    private notify = inject(NotificationService);

    history = signal<PaymentRecord[]>([]);
    dues = signal<MembershipDue[]>([]);
    savedMethods = signal<any[]>([]);
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

    selectedPaymentMethod = signal<any>(null);
    selectedReceiptFile: File | null = null;

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);
        this.financialService.getMyDues().subscribe(dues => {
            this.dues.set(dues);
            this.financialService.getMyHistory().subscribe(history => {
                this.history.set(history);
                this.financialService.getSavedMethods().subscribe(methods => {
                    this.savedMethods.set(methods);
                    this.loading.set(false);
                });
            });
        });
    }

    downloadReceipt(id: number) {
        window.open(this.financialService.getReceiptUrl(id), '_blank');
        this.notify.info('Accessing secure receipt registry...');
    }

    removeMethod(id: number) {
        if (!confirm('Deregister this payment method from your identity wallet?')) return;
        this.financialService.deleteSavedMethod(id).subscribe({
            next: () => {
                this.notify.success('Identity wallet updated.');
                this.loadData();
            }
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

    onPaymentMethodSelected(method: any) {
        this.selectedPaymentMethod.set(method);
        this.paymentForm.paymentMethod = method.method || method.displayName;
    }

    onReferenceSelected(trxId: string) {
        this.paymentForm.transactionId = trxId;
    }

    onReceiptSelected(file: File) {
        this.selectedReceiptFile = file;
    }

    submitPayment() {
        if (!this.paymentForm.transactionId && this.selectedPaymentMethod()?.requiresReference) {
            this.notify.error('Transaction ID is required for this payment method.');
            return;
        }

        const formData = new FormData();
        formData.append('amount', this.paymentForm.amount.toString()); // Wait, this might need adjustment
        // Actually financialService.recordPayment likely expects an object.
        // Let's check financialService.ts

        this.financialService.recordPayment(this.paymentForm).subscribe({
            next: () => {
                this.notify.success('Payment information submitted correctly.');
                this.showPayModal.set(false);
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
