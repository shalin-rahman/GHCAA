import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, firstValueFrom } from 'rxjs';
import { FinancialService, PaymentRecord, MembershipDue } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { PaymentPortalComponent } from '../../common/payment-portal/payment-portal.component';
import {
    FINANCIAL_CATEGORY_OPTIONS,
    getFinancialCategoryLabel,
    getPaymentStatusClass,
    getPaymentStatusLabel
} from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

@Component({
    selector: 'app-payments',
    standalone: true,
    imports: [CommonModule, FormsModule, PaymentPortalComponent, LogoSpinnerComponent, ModalHeaderComponent],
    templateUrl: './payments.html',
    styleUrl: './payments.scss'
})
export class Payments implements OnInit {
    private financialService = inject(FinancialService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

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
        // 29D.4: Load the three sources in parallel with a single error path. The previous
        // nested-subscribe chain had no error callback, so any failure left loading=true
        // forever (infinite spinner). forkJoin resolves/errors once for the whole set.
        forkJoin({
            dues: this.financialService.getMyDues(),
            history: this.financialService.getMyHistory(),
            methods: this.financialService.getSavedMethods()
        }).subscribe({
            next: ({ dues, history, methods }) => {
                this.dues.set(dues);
                this.history.set(history);
                this.savedMethods.set(methods);
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Failed to load payment information. Please try again.');
                this.loading.set(false);
            }
        });
    }

    downloadReceipt(id: number) {
        // 82.32: was window.open(getReceiptUrl(id)) against a URL that didn't exist and, even
        // corrected, sends no auth header to an [Authorize]'d endpoint. Fetch as a blob (the auth
        // interceptor attaches the token) and open that instead.
        this.financialService.getReceipt(id).subscribe({
            next: (blob) => {
                const url = URL.createObjectURL(blob);
                window.open(url, '_blank');
                setTimeout(() => URL.revokeObjectURL(url), 30_000);
            },
            error: () => this.notify.error('Could not load the receipt. Please try again.')
        });
    }

    async removeMethod(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Deregister payment method',
            message: 'Deregister this payment method from your identity wallet?',
            confirmLabel: 'Deregister',
            danger: true
        }));
        if (!ok) return;

        this.financialService.deleteSavedMethod(id).subscribe({
            next: () => {
                this.notify.success('Identity wallet updated.');
                this.loadData();
            },
            // 29F.2: report deletion failures instead of leaving the method silently in place.
            error: () => this.notify.error('Failed to remove the payment method.')
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
