import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LedgerService, FinancialRecord, LedgerSummary } from '../../core/services/ledger.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ledger.html',
  styleUrl: './ledger.scss'
})
export class Ledger implements OnInit {
  private ledgerService = inject(LedgerService);
  private notify = inject(NotificationService);

  transactions = signal<FinancialRecord[]>([]);
  summary = signal<LedgerSummary | null>(null);
  loading = signal(true);
  showForm = signal(false);
  submitting = signal(false);

  newRecord: any = {
    date: new Date().toISOString().split('T')[0],
    amount: 1000,
    type: 0,
    category: 0,
    description: '',
    year: new Date().getFullYear()
  };

  categories = [
    { id: 0, name: 'Membership Fee' },
    { id: 1, name: 'Donation' },
    { id: 2, name: 'Event' },
    { id: 3, name: 'Maintenance' },
    { id: 4, name: 'Salary' },
    { id: 5, name: 'Utilities' },
    { id: 6, name: 'Other' }
  ];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    const year = new Date().getFullYear();
    this.loading.set(true);
    this.ledgerService.getSummary(year).subscribe(s => this.summary.set(s));
    this.ledgerService.getRecords(year).subscribe({
      next: (data) => {
        this.transactions.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getCategoryName(id: any): string {
    const c = this.categories.find(x => x.id == id);
    return c ? c.name : 'Other';
  }

  addRecord() {
    this.submitting.set(true);
    // Ensure year is correct based on date
    this.newRecord.year = new Date(this.newRecord.date).getFullYear();

    this.ledgerService.addRecord(this.newRecord).subscribe({
      next: () => {
        this.notify.success('Financial transaction recorded successfully.');
        this.submitting.set(false);
        this.showForm.set(false);
        this.loadData();
      },
      error: () => {
        this.submitting.set(false);
        this.notify.error('Failed to save transaction. Please check your data.');
      }
    });
  }
}
