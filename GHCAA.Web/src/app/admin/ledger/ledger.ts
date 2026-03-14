import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LedgerService, FinancialRecord, LedgerSummary } from '../../core/services/ledger.service';
import { NotificationService } from '../../core/services/notification.service';
import { ExportButtonsComponent } from '../../common/export-buttons/export-buttons.component';
import { PaginationComponent } from '../../common/pagination/pagination.component';
import { ExportUtil } from '../../core/utils/export.util';

@Component({
  selector: 'app-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule, ExportButtonsComponent, PaginationComponent],
  templateUrl: './ledger.html',
  styleUrl: './ledger.scss'
})
export class Ledger implements OnInit {
  private ledgerService = inject(LedgerService);
  private notify = inject(NotificationService);

  transactions = signal<any[]>([]);
  summary = signal<LedgerSummary | null>(null);
  loading = signal(true);
  showForm = signal(false);
  submitting = signal(false);
  isExporting = signal(false);

  // Pagination & Filtering
  currentPage = signal(1);
  pageSize = signal(10);
  totalItems = signal(0);
  totalPages = signal(1);
  searchQuery = signal('');
  typeFilter = signal<number | null>(null);

  // PDF Export Config
  pdfHeaders = ['Date', 'Type', 'Category', 'Description', 'Amount'];
  pdfMapper = (r: any) => [
    new Date(r.date).toLocaleDateString(),
    r.recordType === 'Income' ? 'Income' : 'Expense',
    this.getCategoryName(r.category),
    r.description,
    r.amount.toLocaleString()
  ];


  newRecord: any = {
    date: new Date().toISOString().split('T')[0],
    amount: 1000,
    recordType: 'Income',
    category: 'MembershipFee',
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
    
    const params: any = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      year: year,
      search: this.searchQuery()
    };
    if (this.typeFilter() !== null) params.type = this.typeFilter();


    this.ledgerService.getRecords(params).subscribe({
      next: (res: any) => {
        this.transactions.set(res.items);
        this.totalItems.set(res.totalItems);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFilterChange() {
    this.currentPage.set(1);
    this.loadData();
  }

  handleExport(format: string) {
    this.isExporting.set(true);
    const year = new Date().getFullYear();
    const params: any = {
      page: 1,
      pageSize: 10000,
      year: year,
      search: this.searchQuery()
    };
    if (this.typeFilter() !== null) params.type = this.typeFilter();

    this.ledgerService.getRecords(params).subscribe({
      next: (res: any) => {
        const data = res.items || [];
        if (format === 'excel') ExportUtil.toExcel(data, 'ghcaa_ledger');
        if (format === 'csv') ExportUtil.toCsv(data, 'ghcaa_ledger');
        if (format === 'pdf') {
          const pData = data.map(this.pdfMapper);
          ExportUtil.toPdf(this.pdfHeaders, pData, 'ghcaa_ledger', 'Financial Ledger Export');
        }
        this.isExporting.set(false);
      },
      error: () => {
        this.isExporting.set(false);
      }
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


