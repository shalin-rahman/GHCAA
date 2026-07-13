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
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-payments',
    standalone: true,
    imports: [CommonModule, FormsModule, PaymentPortalComponent, LogoSpinnerComponent],
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
        if (!this.paymentForm.transactionId) return;

        this.loading.set(true);
        
        // Build FormData for multipart upload (sync with Backend [FromForm])
        const formData = new FormData();
        formData.append('transactionId', this.paymentForm.transactionId);
        formData.append('amount', this.paymentForm.amount.toString());
        formData.append('paidAt', new Date().toISOString());
        formData.append('financialCategory', this.paymentForm.financialCategory.toString());
        formData.append('paymentMethod', this.paymentForm.paymentMethod.toString());
        if (this.paymentForm.notes) formData.append('notes', this.paymentForm.notes);
        
        if (this.selectedReceiptFile) {
            formData.append('receipt', this.selectedReceiptFile, this.selectedReceiptFile.name);
        }

        this.financialService.recordPayment(formData).subscribe({
            next: (res) => {
                this.notify.success('Payment recorded successfully!');
                this.showPayModal.set(false);
                this.loadData();
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Error recording payment');
                this.loading.set(false);
            }
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

    isCompleted(status: any): boolean {
        return status === 'Completed' || status === 1 || status === '1';
    }
}
