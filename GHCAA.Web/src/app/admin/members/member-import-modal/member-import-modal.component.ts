import { Component, inject, signal, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { AdminService } from '../../../core/services/admin.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../../common/logo-spinner/logo-spinner';
import { ModalHeaderComponent } from '../../../common/modal-header/modal-header.component';

export interface SystemProperty {
  value: string;
  label: string;
}

@Component({
  selector: 'app-member-import-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, ModalHeaderComponent],
  templateUrl: './member-import-modal.component.html'
})
export class MemberImportModalComponent {
  private adminService = inject(AdminService);
  private notify = inject(NotificationService);

  @Input() systemProperties: SystemProperty[] = [];
  @Output() closed = new EventEmitter<void>();
  /** Emitted after a successful import so the parent can refresh its list. */
  @Output() imported = new EventEmitter<void>();

  importFile = signal<File | null>(null);
  photoFiles = signal<File[]>([]);
  excelHeaders = signal<string[]>([]);
  isImporting = signal(false);
  columnMapping: Record<string, string> = {};
  defaultValues: Record<string, string> = {};

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.importFile.set(file);
      this.extractHeaders(file);
    }
  }

  onPhotosSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.photoFiles.set(Array.from(input.files ?? []));
  }

  extractHeaders(file: File) {
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      try {
        const data = new Uint8Array(e.target!.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: 'array' });
        const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
        const jsonData = XLSX.utils.sheet_to_json<any>(firstSheet, { header: 1 });

        if (jsonData.length > 0) {
          const headers = (jsonData[0] as any[])
            .filter((h: any) => h != null && String(h).trim() !== '')
            .map((h: any) => String(h).trim());
          this.excelHeaders.set(headers);
          this.columnMapping = {};
          this._autoMap(headers);
          this.notify.success(`Found ${headers.length} columns in the Excel file.`);
        } else {
          this.notify.warning('The Excel file appears to be empty.');
          this.excelHeaders.set([]);
        }
      } catch (err) {
        console.error('Excel parse error:', err);
        this.notify.error("Couldn't read that file. Check that it's a valid .xlsx file.");
        this.excelHeaders.set([]);
      }
    };
    reader.readAsArrayBuffer(file);
  }

  executeImport() {
    const file = this.importFile();
    if (!file) return;

    this.isImporting.set(true);
    const formData = new FormData();
    formData.append('ExcelFile', file);
    this.photoFiles().forEach(f => formData.append('Photos', f));

    // Invert mapping for backend: ExcelColumnName → SystemPropertyName
    const invertedMapping: Record<string, string> = {};
    Object.entries(this.columnMapping).forEach(([sysProp, excelCol]) => {
      if (excelCol && excelCol !== 'undefined') invertedMapping[excelCol] = sysProp;
    });

    formData.append('ColumnMappingJson', JSON.stringify(invertedMapping));
    formData.append('DefaultValuesJson', JSON.stringify(this.defaultValues));

    this.adminService.importMembers(formData).subscribe({
      next: (res: any) => {
        this.notify.success(`Import Complete! Successfully added ${res.successCount} members.`);
        if (res.failureCount > 0) {
          this.notify.warning(`${res.failureCount} rows had issues. Please review the import file.`);
        }
        this.isImporting.set(false);
        this.imported.emit();
        this.closed.emit();
      },
      error: () => {
        this.notify.error('Import failed. Please check file format.');
        this.isImporting.set(false);
      }
    });
  }

  private _autoMap(headers: string[]) {
    headers.forEach(h => {
      const lower = h.toLowerCase();
      if (lower.includes('participant name') || (lower.includes('full') && lower.includes('name')) || lower === 'name') this.columnMapping['FullName'] = h;
      else if (lower.includes('father')) this.columnMapping['FatherName'] = h;
      else if (lower.includes('mother')) this.columnMapping['MotherName'] = h;
      else if (lower.includes('email') || lower.includes('e-mail')) this.columnMapping['Email'] = h;
      else if (lower.includes('mobile') || lower.includes('phone') || lower.includes('cell')) this.columnMapping['MobileNo'] = h;
      else if (lower === 'nid' || lower.includes('national id')) this.columnMapping['NID'] = h;
      else if (lower.includes('batch') || lower.includes('passing year') || lower.includes('session') || lower === 'pass year') this.columnMapping['GHCLastCertificatePassingYear'] = h;
      else if (lower.includes('designation') || lower.includes('position') || lower === 'profession') this.columnMapping['Designation'] = h;
      else if (lower.includes('sector') || lower.includes('profession')) this.columnMapping['ProfessionalSector'] = h;
      else if (lower.includes('blood')) this.columnMapping['BloodGroup'] = h;
      else if (lower.includes('gender') || lower.includes('sex')) this.columnMapping['Gender'] = h;
      else if (lower.includes('dob') || lower.includes('birth')) this.columnMapping['DateOfBirth'] = h;
      else if (lower.includes('present') && lower.includes('address')) this.columnMapping['PresentAddress'] = h;
      else if (lower.includes('permanent') && lower.includes('address')) this.columnMapping['PermanentAddress'] = h;
      else if (lower.includes('address') && !lower.includes('present') && !lower.includes('permanent')) this.columnMapping['PresentAddress'] = h;
      else if (lower.includes('district')) this.columnMapping['PermanentAddress'] = h;
      else if (lower === 'id' || lower === 'sl' || lower === 'serial' || lower.includes('registration')) this.columnMapping['ID'] = h;
      else if (lower.includes('membership') && lower.includes('type')) this.columnMapping['MembershipType'] = h;
      else if (lower.includes('membership') || lower.includes('registration')) this.columnMapping['MembershipNumber'] = h;
      else if (lower.includes('highest') || lower === 'last certificate') this.columnMapping['HighestCertificate'] = h;
      else if ((lower.includes('highest') && lower.includes('group')) || lower === 'group') this.columnMapping['HighestCertificateGroup'] = h;
      else if ((lower.includes('highest') && lower.includes('subject')) || lower === 'department') this.columnMapping['HighestCertificateSubject'] = h;
      else if (lower.includes('certificate') && lower.includes('ghc')) this.columnMapping['GHCLastCertificate'] = h;
      else if (lower.includes('emergency') && lower.includes('name')) this.columnMapping['EmergencyContactName'] = h;
      else if (lower.includes('emergency') && lower.includes('relation')) this.columnMapping['EmergencyContactRelation'] = h;
      else if (lower.includes('emergency') && lower.includes('phone')) this.columnMapping['EmergencyContactPhone'] = h;
      else if (lower.includes('hsc') && lower.includes('admission')) this.columnMapping['HSCAdmissionYear'] = h;
      else if (lower.includes('ghc') && lower.includes('admission')) this.columnMapping['GHCAdmissionYear'] = h;
    });
  }
}
