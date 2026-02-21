import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LedgerService, FinancialRecord, LedgerSummary } from '../../core/services/ledger.service';

@Component({
  selector: 'app-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-feature">
      <div class="header">
        <h2>Financial Ledger</h2>
        <div class="header-btns">
            <button class="btn btn-primary" (click)="showForm.set(!showForm())">
                {{ showForm() ? '✕ Close Form' : '+ Add Record' }}
            </button>
        </div>
      </div>

      <!-- Quick Summary -->
      <div class="balance-cards" *ngIf="!showForm()">
        <div class="glass-card balance-card income">
          <small>Total Income</small>
          <h3>৳ {{ summary()?.totalIncome | number }}</h3>
        </div>
        <div class="glass-card balance-card expense">
          <small>Total Expense</small>
          <h3>৳ {{ summary()?.totalExpense | number }}</h3>
        </div>
        <div class="glass-card balance-card balance">
          <small>Current Balance</small>
          <h3>৳ {{ summary()?.balance | number }}</h3>
        </div>
      </div>

      <!-- Add Record Form -->
      <div class="glass-card form-box" *ngIf="showForm()">
        <h3>Add New Transaction</h3>
        <form (submit)="addRecord()">
            <div class="form-grid">
                <div class="form-group">
                    <label>Date *</label>
                    <input type="date" [(ngModel)]="newRecord.date" name="date" required>
                </div>
                <div class="form-group">
                    <label>Amount (৳) *</label>
                    <input type="number" [(ngModel)]="newRecord.amount" name="amount" required min="1">
                </div>
                <div class="form-group">
                    <label>Type *</label>
                    <select [(ngModel)]="newRecord.type" name="type" required>
                        <option [value]="0">Credit (Income)</option>
                        <option [value]="1">Debit (Expense)</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Category *</label>
                    <select [(ngModel)]="newRecord.category" name="category" required>
                        <option value="0">Membership Fee</option>
                        <option value="1">Donation</option>
                        <option value="2">Event</option>
                        <option value="3">Maintenance</option>
                        <option value="4">Salary</option>
                        <option value="5">Utilities</option>
                        <option value="6">Other</option>
                    </select>
                </div>
                <div class="form-group full-width">
                    <label>Description *</label>
                    <input type="text" [(ngModel)]="newRecord.description" name="desc" required placeholder="Details about this entry">
                </div>
            </div>
            <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="submitting()">
                    {{ submitting() ? 'Saving...' : 'Save Transaction' }}
                </button>
                <button type="button" class="btn btn-secondary" (click)="showForm.set(false)">Cancel</button>
            </div>
        </form>
      </div>

      <!-- Transaction Table -->
      <div class="glass-card table-section" *ngIf="!showForm()">
        @if (loading()) {
            <div class="loading">Loading ledger...</div>
        } @else {
            <table class="admin-table">
            <thead>
                <tr>
                <th>Date</th>
                <th>Description</th>
                <th>Category</th>
                <th>Type</th>
                <th>Amount</th>
                </tr>
            </thead>
            <tbody>
                @for (tr of transactions(); track tr.id) {
                <tr>
                    <td>{{ tr.date | date:'mediumDate' }}</td>
                    <td>{{ tr.description }}</td>
                    <td><span class="category-pill">{{ getCategoryName(tr.category) }}</span></td>
                    <td>
                    <span class="type-indicator" [ngClass]="tr.type === 0 ? 'credit' : 'debit'">
                        {{ tr.type === 0 ? 'Income' : 'Expense' }}
                    </span>
                    </td>
                    <td [ngClass]="tr.type === 0 ? 'credit' : 'debit'">
                    <strong>{{ tr.type === 1 ? '-' : '' }}৳{{ tr.amount | number }}</strong>
                    </td>
                </tr>
                }
            </tbody>
            </table>
        }
      </div>
    </div>
  `,
  styles: [`
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }
    .balance-cards { display: flex; gap: 1.5rem; margin-bottom: 3rem; }
    .balance-card { padding: 1.5rem 2rem; min-width: 180px; text-align: center; }
    .income h3 { color: #2e7d32; }
    .expense h3 { color: #c62828; }
    .balance h3 { color: var(--primary-color); }

    .form-box { padding: 2.5rem; margin-bottom: 3rem; animation: slideDown 0.3s ease; background: var(--surface-color); }
    .form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.5rem; margin-top: 1.5rem; }
    .full-width { grid-column: span 2; }
    .form-group label { display: block; font-weight: 700; font-size: 0.8rem; margin-bottom: 0.5rem; color: var(--text-muted); }
    .form-group input, .form-group select { width: 100%; padding: 0.8rem; border: 1px solid var(--glass-border); border-radius: 8px; background: var(--bg-color); color: var(--text-main); }
    .form-actions { display: flex; gap: 1rem; margin-top: 2rem; }

    .admin-table { width: 100%; border-collapse: collapse; }
    .admin-table th { padding: 1.25rem; background: var(--surface-color); text-align: left; font-size: 0.8rem; text-transform: uppercase; color: var(--text-muted); }
    .admin-table td { padding: 1.25rem; border-bottom: 1px solid var(--glass-border); color: var(--text-main); }

    .category-pill { background: var(--surface-color); padding: 0.2rem 0.6rem; border-radius: 4px; font-size: 0.75rem; color: var(--text-muted); font-weight: 700; border: 1px solid var(--glass-border); }
    .type-indicator { font-weight: 800; font-size: 0.75rem; text-transform: uppercase; }
    .credit { color: #2e7d32; }
    .debit { color: #c62828; }

    @keyframes slideDown { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }

    @media (max-width: 992px) {
      .balance-cards { flex-wrap: wrap; }
      .balance-card { flex: 1; }
      .form-grid { grid-template-columns: 1fr; }
      .full-width { grid-column: span 1; }
    }
  `]
})
export class Ledger implements OnInit {
  private ledgerService = inject(LedgerService);

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
        alert('Transaction recorded successfully!');
        this.submitting.set(false);
        this.showForm.set(false);
        this.loadData();
      },
      error: () => {
        this.submitting.set(false);
        alert('Error adding record. Check all fields.');
      }
    });
  }
}
