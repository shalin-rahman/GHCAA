import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExportUtil } from '../../core/utils/export.util';

@Component({
  selector: 'app-export-buttons',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="export-bar">
      <button type="button" (click)="onExport('excel')" [disabled]="exporting" class="btn-export excel" title="Export to Excel">
        <span *ngIf="!exporting">📊</span>
        <span *ngIf="exporting" class="spin">⏳</span>
        Excel
      </button>
      <button type="button" (click)="onExport('csv')" [disabled]="exporting" class="btn-export csv" title="Export to CSV">
        <span *ngIf="!exporting">📄</span>
        <span *ngIf="exporting" class="spin">⏳</span>
        CSV
      </button>
      <button type="button" (click)="onExport('pdf')" [disabled]="exporting" class="btn-export pdf" title="Export to PDF">
        <span *ngIf="!exporting">🎬</span>
        <span *ngIf="exporting" class="spin">⏳</span>
        PDF
      </button>
    </div>
  `,
  styles: [`
    .export-bar {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .btn-export {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 0.6rem;
      font-size: 0.72rem;
      font-weight: 800;
      letter-spacing: 0.03em;
      text-transform: uppercase;
      cursor: pointer;
      transition: all 0.2s ease;
      box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    }

    .btn-export:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 14px rgba(0,0,0,0.25);
    }

    .btn-export:active:not(:disabled) {
      transform: translateY(0);
      box-shadow: 0 1px 4px rgba(0,0,0,0.2);
    }

    .btn-export:disabled {
      opacity: 0.4;
      cursor: not-allowed;
      transform: none;
      box-shadow: none;
    }

    .btn-export.excel {
      background: linear-gradient(135deg, #16a34a, #15803d);
      color: #fff;
    }

    .btn-export.csv {
      background: linear-gradient(135deg, #2563eb, #1d4ed8);
      color: #fff;
    }

    .btn-export.pdf {
      background: linear-gradient(135deg, #dc2626, #b91c1c);
      color: #fff;
    }

    .spin {
      display: inline-block;
      animation: spin 1s linear infinite;
      font-size: 10px;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
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
    // Always emit if parent has bound to exportStart
    this.exportStart.emit(format);
  }
}
