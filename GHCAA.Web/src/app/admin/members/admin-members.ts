import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { Router, ActivatedRoute } from '@angular/router';
import { EC_ROLES, getECPositionName, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getStatusLabel, getStatusClass, getCategoryLabel, getMembershipTypeLabel, MEMBERSHIP_STATUS_MAP, MEMBERSHIP_STATUS_OPTIONS, MEMBERSHIP_TYPE_OPTIONS, MEMBER_CATEGORY_OPTIONS, EC_ROLES_OPTIONS, GENDER_OPTIONS, BLOOD_GROUP_OPTIONS, getBloodGroupName } from '../../core/constants/app.constants';
import * as XLSX from 'xlsx';
import { ExportButtonsComponent } from '../../common/export-buttons/export-buttons.component';
import { PaginationComponent } from '../../common/pagination/pagination.component';
import { ExportUtil } from '../../core/utils/export.util';

@Component({
  selector: 'app-admin-members',
  standalone: true,
  imports: [CommonModule, FormsModule, ExportButtonsComponent, PaginationComponent],
  templateUrl: './admin-members.html',
  styleUrl: './admin-members.scss'
})
export class AdminMembers implements OnInit {
  private adminService = inject(AdminService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  nav = inject(NavService);

  allMembers = signal<any[]>([]);
  loading = signal(true);
  isExporting = signal(false);
  searchQuery = signal('');
  statusFilter = signal('all');
  selectedMember = signal<any>(null);
  isEditing = signal(false);
  certToUpload: File | null = null;
  payToUpload: File | null = null;
  photoToUpload: File | null = null;
  photoPreview = signal<string | null>(null);
  memberPayments = signal<any[]>([]);

  // Pagination state
  currentPage = signal(1);
  pageSize = signal(10);
  totalPages = signal(1);
  totalItems = signal(0);

  // Governance
  ecPeriods = signal<any[]>([]);

  // Import State
  showImportModal = signal(false);
  isImporting = signal(false);
  importFile = signal<File | null>(null);
  photoFiles = signal<File[]>([]);
  excelHeaders = signal<string[]>([]);
  columnMapping: Record<string, string> = {
    'FullName': '',
    'Email': '',
    'MobileNo': '',
    'NID': '',
    'GHCLastCertificatePassingYear': '',
    'ID': ''
  };

  defaultValues: Record<string, string> = {};

  systemProperties = [
    // Personal
    { value: 'FullName', label: 'Full Name' },
    { value: 'FatherName', label: "Father's Name" },
    { value: 'MotherName', label: "Mother's Name" },
    { value: 'DateOfBirth', label: 'Date of Birth' },
    { value: 'Gender', label: 'Gender' },
    { value: 'BloodGroup', label: 'Blood Group' },
    { value: 'NID', label: 'NID Number' },
    { value: 'MobileNo', label: 'Mobile Number' },
    { value: 'Email', label: 'Email Address' },
    { value: 'PresentAddress', label: 'Present Address' },
    { value: 'PermanentAddress', label: 'Permanent Address' },
    { value: 'EmergencyContactName', label: 'Emergency Contact Name' },
    { value: 'EmergencyContactRelation', label: 'Emergency Contact Relation' },
    { value: 'EmergencyContactPhone', label: 'Emergency Contact Phone' },
    // Academic — Highest
    { value: 'HighestCertificate', label: 'Highest Certificate' },
    { value: 'HighestCertificateGroup', label: 'Highest Certificate Group' },
    { value: 'HighestCertificateSubject', label: 'Highest Certificate Subject' },
    { value: 'HighestCertificatePassingYear', label: 'Highest Certificate Passing Year' },
    { value: 'HSCAdmissionYear', label: 'HSC Admission Year' },
    // Academic — GHC
    { value: 'GHCLastCertificate', label: 'GHC Last Certificate' },
    { value: 'GHCLastCertificateGroup', label: 'GHC Last Certificate Group' },
    { value: 'GHCLastCertificateSubject', label: 'GHC Last Certificate Subject' },
    { value: 'GHCLastCertificatePassingYear', label: 'GHC Passing Year (Batch)' },
    { value: 'GHCAdmissionYear', label: 'GHC Admission Year' },
    // Professional
    { value: 'ProfessionalSector', label: 'Professional Sector' },
    { value: 'Designation', label: 'Professional Designation' },
    // Membership
    { value: 'MembershipNumber', label: 'Membership Number' },
    { value: 'MembershipType', label: 'Membership Type' },
    { value: 'Category', label: 'Member Category' },
    // System
    { value: 'ID', label: 'System/External ID (For Photos)' }
  ];

  // Constants for dropdowns
  membershipTypes = MEMBERSHIP_TYPE_OPTIONS;
  memberCategories = MEMBER_CATEGORY_OPTIONS;
  statusOptions = MEMBERSHIP_STATUS_OPTIONS;
  ecPositions = EC_ROLES_OPTIONS;
  genderOptions = GENDER_OPTIONS;
  bloodGroupOptions = BLOOD_GROUP_OPTIONS;

  ACADEMIC = ACADEMIC_DATA;
  yearsList = this.ACADEMIC.getYears();
  IS_HSC = IS_HSC;
  degreeOptions = this.ACADEMIC.certificates;
  groupOptions = this.ACADEMIC.groups;
  subjectOptions = this.ACADEMIC.subjects;
  sectorOptions = this.ACADEMIC.sectors;

  // PDF Export Config
  pdfHeaders = ['ID', 'Name', 'Email', 'Mobile', 'Batch', 'Status'];
  pdfMapper = (m: any) => [
    m.membershipNumber || `M-${m.id}`,
    m.fullName,
    m.email,
    m.mobileNo,
    m.ghcLastCertificatePassingYear,
    m.status
  ];

  ngOnInit() {
    this.loadMembers();
    this.loadECPeriods();
  }

  loadECPeriods() {
    this.adminService.getPeriods().subscribe({
      next: (periods) => this.ecPeriods.set(periods),
      error: () => this.ecPeriods.set([])
    });
  }

  onFilterChange() {
    this.currentPage.set(1);
    this.loadMembers();
  }

  handleExport(format: string) {
    this.isExporting.set(true);
    this.adminService.getAllForExport(this.searchQuery(), this.statusFilter()).subscribe({
      next: async (res: any) => {
        const rawData = res.items || res;
        
        // Flatten the data for Excel/CSV so it doesn't try to parse nested JSON objects which corrupts the file
        const flatData = rawData.map((m: any) => ({
          'Membership No': m.membershipNumber || `M-${m.id}`,
          'Name': m.fullName,
          'Email': m.email,
          'Mobile': m.mobileNo,
          'Batch': m.ghcLastCertificatePassingYear || '—',
          'Type': getMembershipTypeLabel(m.membershipType),
          'Status': getStatusLabel(m.status),
          'Blood Group': getBloodGroupName(m.bloodGroup) || '—',
          'Organization': m.professionalSector || '—',
          'Designation': m.designation || '—'
        }));

        if (format === 'excel') ExportUtil.toExcel(flatData, 'ghcaa_members');
        if (format === 'csv') ExportUtil.toCsv(flatData, 'ghcaa_members');
        if (format === 'pdf') {
          const pData = rawData.map(this.pdfMapper);
          await ExportUtil.toPdf(this.pdfHeaders, pData, 'ghcaa_members', 'Member Registry Export');
        }
        this.isExporting.set(false);
      },
      error: () => {
        this.notify.error('Failed to prepare export data');
        this.isExporting.set(false);
      }
    });
  }

  loadMembers() {
    this.loading.set(true);
    this.adminService.getMembers(this.currentPage(), this.pageSize(), this.searchQuery(), this.statusFilter(), false).subscribe({
      next: (res: any) => {
        this.allMembers.set(res.items);
        this.totalPages.set(res.totalPages || 1);
        this.totalItems.set(res.totalItems || 0);
        this.loading.set(false);
      },
      error: () => {
        this.allMembers.set([]);
        this.loading.set(false);
      }
    });
  }

  changePage(delta: number) {
    const dest = this.currentPage() + delta;
    if (dest >= 1 && dest <= this.totalPages()) {
      this.currentPage.set(dest);
      this.loadMembers();
    }
  }

  setPageListOptions(size: number) {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.loadMembers();
  }

  approveMember(id: number, adminId: number) {
    this.adminService.approveMember(id, adminId).subscribe({
      next: (res: any) => {
        this.notify.success(`Approved! Membership: ${res.membershipNumber}`);
        this.loadMembers();
      },
      error: () => this.notify.error('Approval failed.')
    });
  }

  archiveMember(id: number) {
    if (!confirm('Archive this member? This action is reversible.')) return;
    this.adminService.archiveMember(id).subscribe({
      next: () => { this.notify.success('Member archived.'); this.loadMembers(); },
      error: () => this.notify.error('Archive failed.')
    });
  }

  sendResetLink(id: number) {
    if (!confirm('Send a password reset link to this member?')) return;
    this.adminService.sendPasswordResetLink(id).subscribe({
      next: () => this.notify.success('Password reset link sent.'),
      error: () => this.notify.error('Failed to send reset link.')
    });
  }

  openDetail(member: any) { 
    this.selectedMember.set({ ...member }); 
    this.isEditing.set(false); 
    this.photoToUpload = null; 
    this.photoPreview.set(null); 
    this.loadPayments(member.id);
  }

  loadPayments(memberId: number) {
    this.adminService.getMemberPayments(memberId).subscribe({
      next: (res) => this.memberPayments.set(res),
      error: () => this.memberPayments.set([])
    });
  }

  closeDetail() { this.selectedMember.set(null); this.isEditing.set(false); this.photoToUpload = null; this.photoPreview.set(null); this.memberPayments.set([]); }

  toggleEdit() { this.isEditing.update(v => !v); }

  onAdminPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.photoToUpload = file;
    const reader = new FileReader();
    reader.onload = (e) => this.photoPreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  uploadMemberPhoto() {
    const member = this.selectedMember();
    if (!member || !this.photoToUpload) return;
    this.adminService.updateMemberPhoto(member.id, this.photoToUpload).subscribe({
      next: (res) => {
        this.selectedMember.update(m => ({ ...m, photoPath: res.photoPath }));
        this.photoToUpload = null;
        this.photoPreview.set(null);
        this.notify.success('Photo updated successfully!');
        this.loadMembers();
      },
      error: (err) => this.notify.error(err?.error?.message || 'Photo upload failed.')
    });
  }

  saveMember() {
    const member = this.selectedMember();
    if (!member) return;

    if (!member.fullName || !member.mobileNo || !member.email) {
      this.notify.error('Member profile requires at least a Name, Mobile, and Email.');
      return;
    }

    ensureValidAcademicData(member);

    this.adminService.updateMember(member.id, {
      fullName: member.fullName,
      fatherName: member.fatherName,
      motherName: member.motherName,
      dateOfBirth: member.dateOfBirth,
      nid: member.nid,
      mobileNo: member.mobileNo,
      email: member.email,
      gender: member.gender,
      bloodGroup: member.bloodGroup,
      presentAddress: member.presentAddress,
      permanentAddress: member.permanentAddress,
      hscAdmissionYear: member.hscAdmissionYear,
      highestCertificate: member.highestCertificate,
      highestCertificateGroup: member.highestCertificateGroup,
      highestCertificateSubject: member.highestCertificateSubject,
      highestCertificatePassingYear: member.highestCertificatePassingYear,
      ghcAdmissionYear: member.ghcAdmissionYear,
      ghcLastCertificate: member.ghcLastCertificate,
      ghcLastCertificateGroup: member.ghcLastCertificateGroup,
      ghcLastCertificateSubject: member.ghcLastCertificateSubject,
      ghcLastCertificatePassingYear: member.ghcLastCertificatePassingYear,
      professionalSector: member.professionalSector,
      designation: member.designation,
      membershipType: member.membershipType,
      category: member.category,
      ecPosition: member.ecPosition,
      membershipNumber: member.membershipNumber,
      isMobilePublic: member.isMobilePublic,
      isEmailPublic: member.isEmailPublic,
      isAddressPublic: member.isAddressPublic,
      academicHistory: member.academicHistory,
      professionalHistory: member.professionalHistory
    }).subscribe({
      next: () => {
        // After data update, if there are files, upload them
        if (this.certToUpload || this.payToUpload) {
          this.adminService.updateMemberDocuments(member.id, this.certToUpload || undefined, this.payToUpload || undefined).subscribe({
            next: () => {
              this.notify.success('Member information and documents updated.');
              this.finalizeSave();
            },
            error: () => this.notify.error('Data updated, but document upload failed.')
          });
        } else {
          this.notify.success('Member information updated.');
          this.finalizeSave();
        }
      },
      error: () => this.notify.error('Update failed.')
    });
  }

  private finalizeSave() {
    const memberId = this.selectedMember()?.id;
    this.isEditing.set(false);
    this.certToUpload = null;
    this.payToUpload = null;
    this.loadMembers();
    
    // Refresh selected member details to show new paths/data
    if (memberId) {
      this.adminService.getMembers(1, 1, `M-${memberId}`, 'all').subscribe({
        next: (res: any) => {
          if (res.items && res.items.length > 0) {
            this.selectedMember.set(res.items[0]);
          }
        }
      });
    }
  }

  onDocSelected(event: any, type: 'cert' | 'pay') {
    const file = event.target.files[0];
    if (file) {
      if (type === 'cert') this.certToUpload = file;
      else this.payToUpload = file;
    }
  }

  contactMember(email: string) {
    this.router.navigate(['/admin/comm'], { queryParams: { target: email, method: 'custom' } });
  }

  getStatusLabel = getStatusLabel;
  getStatusClass = getStatusClass;
  getCategoryLabel = getCategoryLabel;
  getMembershipTypeLabel = getMembershipTypeLabel;
  getECPositionLabel = getECPositionName;
  getBloodGroupName = getBloodGroupName;

  // --- Import Actions ---
  openImport() {
    this.showImportModal.set(true);
    this.importFile.set(null);
    this.photoFiles.set([]);
    this.excelHeaders.set([]);
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.importFile.set(file);
      this.extractHeaders(file);
    }
  }

  onPhotosSelected(event: any) {
    this.photoFiles.set(Array.from(event.target.files));
  }

  extractHeaders(file: File) {
    const reader = new FileReader();
    reader.onload = (e: any) => {
      try {
        const data = new Uint8Array(e.target.result);
        const workbook = XLSX.read(data, { type: 'array' });
        const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
        const jsonData = XLSX.utils.sheet_to_json<any>(firstSheet, { header: 1 });

        if (jsonData.length > 0) {
          // First row = headers
          const headers = (jsonData[0] as any[]).filter((h: any) => h != null && String(h).trim() !== '').map((h: any) => String(h).trim());
          this.excelHeaders.set(headers);

          // Reset mappings
          this.columnMapping = {};

          // Auto-map by fuzzy matching
          headers.forEach(h => {
            const lower = h.toLowerCase();
            if (lower.includes('participant name') || (lower.includes('full') && lower.includes('name')) || lower === 'name') this.columnMapping['FullName'] = h;
            else if (lower.includes('father')) this.columnMapping['FatherName'] = h;
            else if (lower.includes('mother')) this.columnMapping['MotherName'] = h;
            else if (lower.includes('email') || lower.includes('e-mail')) this.columnMapping['Email'] = h;
            else if (lower.includes('mobile') || lower.includes('phone') || lower.includes('cell')) this.columnMapping['MobileNo'] = h;
            else if (lower === 'nid' || lower.includes('national id')) this.columnMapping['NID'] = h;
            else if (lower.includes('batch') || lower.includes('passing year') || lower.includes('session') || lower === 'pass year') this.columnMapping['GHCLastCertificatePassingYear'] = h;
            else if (lower.includes('batch') || lower.includes('passing year') || lower.includes('session') || lower === 'pass year') this.columnMapping['HighestCertificatePassingYear'] = h;
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
            else if (lower.includes('membership') || lower.includes('registration')) this.columnMapping['MembershipNumber'] = h;
            else if (lower.includes('membership') && lower.includes('type')) this.columnMapping['MembershipType'] = h;
            else if (lower.includes('highest') || lower === 'last certificate') this.columnMapping['HighestCertificate'] = h;
            else if (lower.includes('highest') && lower.includes('group') || lower === 'group') this.columnMapping['HighestCertificateGroup'] = h;
            else if (lower.includes('highest') && lower.includes('subject') || lower === 'department') this.columnMapping['HighestCertificateSubject'] = h;
            else if (lower.includes('certificate') && lower.includes('ghc')) this.columnMapping['GHCLastCertificate'] = h;
            else if (lower.includes('emergency') && lower.includes('name')) this.columnMapping['EmergencyContactName'] = h;
            else if (lower.includes('emergency') && lower.includes('relation')) this.columnMapping['EmergencyContactRelation'] = h;
            else if (lower.includes('emergency') && lower.includes('phone')) this.columnMapping['EmergencyContactPhone'] = h;
            else if (lower.includes('hsc') && lower.includes('admission')) this.columnMapping['HSCAdmissionYear'] = h;
            else if (lower.includes('ghc') && lower.includes('admission')) this.columnMapping['GHCAdmissionYear'] = h;
          });

          this.notify.success(`Found ${headers.length} columns in the Excel file.`);
        } else {
          this.notify.warning('The Excel file appears to be empty.');
          this.excelHeaders.set([]);
        }
      } catch (err) {
        console.error('Excel parse error:', err);
        this.notify.error('Failed to read Excel file. Please ensure it is a valid .xlsx file.');
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

    // Invert mapping for backend: ExcelColumnName -> SystemPropertyName
    const invertedMapping: Record<string, string> = {};
    Object.entries(this.columnMapping).forEach(([sysProp, excelCol]) => {
      if (excelCol && excelCol !== 'undefined') {
        invertedMapping[excelCol] = sysProp;
      }
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
        this.showImportModal.set(false);
        this.loadMembers();
      },
      error: () => {
        this.notify.error('Import failed. Please check file format.');
        this.isImporting.set(false);
      }
    });
  }

  addAcademic() {
    if (!this.selectedMember().academicHistory) this.selectedMember().academicHistory = [];
    this.selectedMember().academicHistory.unshift({
      institutionName: '',
      degree: '',
      subject: '',
      admissionYear: new Date().getFullYear() - 4,
      passingYear: new Date().getFullYear(),
      isGHC: false,
      result: ''
    });
  }

  removeAcademic(index: number) {
    this.selectedMember().academicHistory.splice(index, 1);
  }

  addProfessional() {
    if (!this.selectedMember().professionalHistory) this.selectedMember().professionalHistory = [];
    this.selectedMember().professionalHistory.unshift({
      organizationName: '',
      designation: '',
      sector: '',
      location: '',
      startDate: new Date().toISOString().split('T')[0],
      isCurrent: true
    });
  }

  removeProfessional(index: number) {
    this.selectedMember().professionalHistory.splice(index, 1);
  }

  addECHistory() {
    if (!this.selectedMember().ecHistory) this.selectedMember().ecHistory = [];
    const activePeriod = this.ecPeriods().find(p => p.isActive);
    this.selectedMember().ecHistory.unshift({
      periodTitle: activePeriod ? activePeriod.title : '',
      position: 0, // None
      startDate: new Date().toISOString().split('T')[0],
      isCurrent: true,
      changeReason: ''
    });
  }

  deletePayment(paymentId: number) {
    if (!confirm('Permanently delete this payment record? This will also mark linked dues as unpaid.')) return;
    this.adminService.deletePayment(paymentId).subscribe({
      next: () => {
        this.notify.success('Payment deleted.');
        this.loadPayments(this.selectedMember().id);
      },
      error: () => this.notify.error('Failed to delete payment.')
    });
  }

  deleteECHistory(id: number) {
    if (!confirm('Permanently delete this EC role history record?')) return;
    this.adminService.deleteECMember(id).subscribe({
      next: () => {
        this.notify.success('EC History deleted.');
        this.finalizeSave(); // Refresh data
      },
      error: () => this.notify.error('Failed to delete history.')
    });
  }
}


