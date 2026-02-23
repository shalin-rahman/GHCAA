import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService, PaymentRecord, MembershipDue } from '../../core/services/financial.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-payments',
    standalone: true,
    imports: [CommonModule, FormsModule],
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

    paymentForm = {
        amount: 0,
        transactionId: '',
        notes: ''
    };

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
            notes: due ? `Annual Dues for ${due.year}` : ''
        };
        this.showPayModal.set(true);
    }

    submitPayment() {
        if (!this.paymentForm.transactionId) return;
        this.financialService.recordPayment(this.paymentForm).subscribe({
            next: () => {
                this.notify.success('Payment information submitted correctly. Status will be updated after verification.');
                this.showPayModal.set(false);
                this.loadData();
            },
            error: () => this.notify.error('Failed to submit payment.')
        });
    }

    getStatusClass(status: any): string {
        const map: Record<string, string> = { '0': 'pending', '1': 'success', '2': 'failed' };
        return map[String(status)] || '';
    }

    getStatusLabel(status: any): string {
        const map: Record<string, string> = { '0': 'Pending Audit', '1': 'Verified', '2': 'Rejected' };
        return map[String(status)] || 'Unknown';
    }
}
