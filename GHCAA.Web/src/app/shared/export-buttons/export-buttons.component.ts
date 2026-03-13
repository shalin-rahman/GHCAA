import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExportUtil } from '../../core/utils/export.util';

@Component({
  selector: 'app-export-buttons',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center gap-2">
      <button (click)="onExport('excel')" [disabled]="exporting" class="btn-export border-green-600/30 text-green-500 hover:bg-green-600/10" title="Export to Excel">
        <span *ngIf="!exporting">📊</span> 
        <span *ngIf="exporting" class="animate-spin text-[10px]">⏳</span>
        <span class="hidden sm:inline">Excel</span>
      </button>
      <button (click)="onExport('csv')" [disabled]="exporting" class="btn-export border-blue-600/30 text-blue-500 hover:bg-blue-600/10" title="Export to CSV">
        <span *ngIf="!exporting">📄</span> 
        <span *ngIf="exporting" class="animate-spin text-[10px]">⏳</span>
        <span class="hidden sm:inline">CSV</span>
      </button>
      <button (click)="onExport('pdf')" [disabled]="exporting" class="btn-export border-red-600/30 text-red-500 hover:bg-red-600/10" title="Export to PDF">
        <span *ngIf="!exporting">🎬</span> 
        <span *ngIf="exporting" class="animate-spin text-[10px]">⏳</span>
        <span class="hidden sm:inline">PDF</span>
      </button>
    </div>
  `,
  styles: [`
    .btn-export {
      @apply flex items-center gap-2 px-3 py-1.5 border rounded-lg text-xs font-bold transition-all hover:scale-105 active:scale-95 bg-white/5;
    }
    .btn-export:disabled {
      @apply opacity-50 cursor-not-allowed scale-100;
    }
  `]
})
export class ExportButtonsComponent {
  @Input() data: any[] = [];
  @Input() fileName: string = 'export';
  @Input() pdfHeaders: string[] = [];
  @Input() pdfDataMapper: (item: any) => any[] = (item) => Object.values(item);
  @Input() title: string = 'Data Export';
  @Input() exporting: boolean = false;
  @Output() exportStart = new EventEmitter<string>();

  onExport(format: string) {
    if (this.exportStart.observers.length > 0) {
      this.exportStart.emit(format);
    } else {
      this.doExport(format, this.data);
    }
  }

  doExport(format: string, exportData: any[]): void {
    if (format === 'excel') ExportUtil.toExcel(exportData, this.fileName);
    if (format === 'csv') ExportUtil.toCsv(exportData, this.fileName);
    if (format === 'pdf') {
      const pData = exportData.map(this.pdfDataMapper);
      ExportUtil.toPdf(this.pdfHeaders, pData, this.fileName, this.title);
    }
  }
}
