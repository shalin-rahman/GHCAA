import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { Router, ActivatedRoute } from '@angular/router';
import { EC_ROLES, getECPositionName, getCurrentECPosition, getCurrentECPeriod, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getStatusLabel, getStatusClass, getCategoryLabel, getMembershipTypeLabel, MEMBERSHIP_STATUS_MAP, MEMBERSHIP_STATUS_OPTIONS, MEMBERSHIP_TYPE_OPTIONS, MEMBER_CATEGORY_OPTIONS, EC_ROLES_OPTIONS, GENDER_OPTIONS, BLOOD_GROUP_OPTIONS, getBloodGroupName } from '../../core/constants/app.constants';
import { DatePipe } from '@angular/common';
import * as XLSX from 'xlsx';
import { ExportButtonsComponent } from '../../common/export-buttons/export-buttons.component';
import { PaginationComponent } from '../../common/pagination/pagination.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ExportUtil } from '../../core/utils/export.util';
import { validateUploadFile } from '../../core/utils/file-validation.util';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { toWireDate } from '../../core/utils/date.util';
import { Icon } from '../../common/icon/icon';

@Component({
  selector: 'app-admin-members',
  standalone: true,
  imports: [CommonModule, FormsModule, ExportButtonsComponent, PaginationComponent, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective, Icon],
  providers: [DatePipe],
  templateUrl: './admin-members.html',
  styleUrl: './admin-members.scss'
})
export class AdminMembers implements OnInit {
  private adminService = inject(AdminService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private datePipe = inject(DatePipe);
  nav = inject(NavService);

  allMembers = signal<any[]>([]);
  loading = signal(true);
  isExporting = signal(false);
  searchQuery = signal('');
  statusFilter = signal('all');
  categoryFilter = signal('all');
  membershipTypeFilter = signal('all');
  includeArchived = signal(false);
  selectedMember = signal<any>(null);
  isEditing = signal(false);
  submitting = signal(false);
  certToUpload: File | null = null;
  payToUpload: File | null = null;
  photoToUpload: File | null = null;
  signatureToUpload: File | null = null;
  photoPreview = signal<string | null>(null);
  signaturePreview = signal<string | null>(null);
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
    { value: 'ID', label: 'System/External ID (For Photos)' },
    // Privacy & Notifications
    { value: 'IsMobilePublic', label: 'Expose Mobile' },
    { value: 'IsEmailPublic', label: 'Expose Email' },
    { value: 'IsAddressPublic', label: 'Expose Address' },
    { value: 'IsNIDPublic', label: 'Expose NID' },
    { value: 'IsFamilyPublic', label: 'Expose Family' },
    { value: 'NotifyEventCreation', label: 'Notify Events' },
    { value: 'NotifyRelevantUpdates', label: 'Notify Relevant News' },
    // Status
    { value: 'IsVerified', label: 'Verified Alumni (True/False)' },
    { value: 'ContributionPoints', label: 'Merit Points' }
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

  getImageUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return cleanPath.replace(/^\/\//, '/');
  }

  ngOnInit() {
    this.loadMembers();
    this.loadECPeriods();
  }

  loadECPeriods() {
    this.adminService.getPeriods().subscribe({
      next: (periods: any[]) => this.ecPeriods.set(periods),
      error: () => this.ecPeriods.set([])
    });
  }

  onFilterChange() {
    this.currentPage.set(1);
    this.loadMembers();
  }

  handleExport(format: string) {
    this.isExporting.set(true);
    this.adminService.getAllForExport(this.searchQuery(), this.statusFilter(), this.categoryFilter(), this.membershipTypeFilter()).subscribe({
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
    const page = this.currentPage();
    const query = this.searchQuery();
    const status = this.statusFilter();
    const type = this.membershipTypeFilter();
    const cat = this.categoryFilter();
    const incArchived = this.includeArchived();

    this.adminService.getMembers(page, this.pageSize(), query, status, cat, type, incArchived).subscribe({
      next: (res: any) => {
        // 32.3: generic case-insensitive key normalization — see openDetail() below for why
        // a hardcoded field whitelist was the bug, not the fix.
        const mapping = (obj: any) => {
          const result: any = {};
          Object.keys(obj || {}).forEach(key => {
            const camelKey = key === key.toUpperCase()
              ? key.toLowerCase()
              : key.charAt(0).toLowerCase() + key.slice(1);
            if (result[camelKey] === undefined) {
              result[camelKey] = obj[key];
            }
          });
          return result;
        };

        const mappedItems = (res.items || []).map(mapping);
        this.allMembers.set(mappedItems);
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

  approveMember(id: number) {
    this.adminService.approveMember(id).subscribe({
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

  restoreMember(id: number) {
    if (!confirm('Restore this member from the archive?')) return;
    this.adminService.restoreMember(id).subscribe({
      next: () => { this.notify.success('Member restored.'); this.loadMembers(); },
      error: () => this.notify.error('Restore failed.')
    });
  }

  sendResetLink(id: number) {
    if (!confirm('Send a password reset link to this member?')) return;
    this.adminService.sendPasswordResetLink(id).subscribe({
      next: (res: any) => {
        if (res.resetUrl) {
            if (navigator.clipboard) {
                navigator.clipboard.writeText(res.resetUrl).then(() => {
                    this.notify.success('Link copied to dashboard clipboard automatically.');
                }).catch(() => {
                    prompt('Password Reset Link:', res.resetUrl);
                });
            } else {
                prompt('Password Reset Link:', res.resetUrl);
            }
        } else {
            this.notify.success(res.message || 'Password reset link sent.');
        }
      },
      error: (err: any) => this.notify.error(err.error?.message || 'Failed to send reset link.')
    });
  }

  openDetail(member: any) { 
    this.loading.set(true);
    this.adminService.getMemberById(member.id).subscribe({
      next: (fullMember: any) => {
        // 32.3: generic case-insensitive key normalization (handles both PascalCase C# DTOs and
        // camelCase JS) — was a hardcoded whitelist that silently dropped any unlisted flat field
        // (designation, professionalSector, profileCompletionPercentage, etc.)
        const mapping = (obj: any) => {
          const result: any = {};
          Object.keys(obj || {}).forEach(key => {
            const camelKey = key === key.toUpperCase()
              ? key.toLowerCase()
              : key.charAt(0).toLowerCase() + key.slice(1);
            if (result[camelKey] === undefined) {
              result[camelKey] = obj[key];
            }
          });

          // Copy nested collections directly if they exist
          result.academicHistory = obj.academicHistory || obj.AcademicHistory || [];
          result.professionalHistory = obj.professionalHistory || obj.ProfessionalHistory || [];
          result.ecHistory = obj.ecHistory || obj.ECHistory || [];

          return result;
        };

        const mappedMember = mapping(fullMember);

        // Date normalization for Registry standards (dd-MM-yyyy)
        if (mappedMember.dateOfBirth) {
            mappedMember.dateOfBirth = this.datePipe.transform(mappedMember.dateOfBirth, 'dd-MM-yyyy') || '';
        } else if (fullMember.DateOfBirth) {
            mappedMember.dateOfBirth = this.datePipe.transform(fullMember.DateOfBirth, 'dd-MM-yyyy') || '';
        }

        if (mappedMember.professionalHistory) {
            mappedMember.professionalHistory = mappedMember.professionalHistory.map((ph: any) => ({
                ...ph,
                startDate: this.datePipe.transform(ph.startDate || ph.StartDate, 'dd-MM-yyyy') || ''
            }));
        }

        this.selectedMember.set(mappedMember);
        this.isEditing.set(false); 
        this.photoToUpload = null; 
        this.photoPreview.set(null); 
        this.loadPayments(member.id);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load member profile.');
        this.loading.set(false);
      }
    });
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

  onAdminSignatureSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.signatureToUpload = file;
    const reader = new FileReader();
    reader.onload = (e) => this.signaturePreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  uploadMemberSignature() {
    const member = this.selectedMember();
    if (!member || !this.signatureToUpload) return;
    this.adminService.updateMemberSignature(member.id, this.signatureToUpload).subscribe({
      next: (res) => {
        this.selectedMember.update(m => ({ ...m, signaturePath: res.signaturePath }));
        this.signatureToUpload = null;
        this.signaturePreview.set(null);
        this.notify.success('Signature updated successfully!');
        this.loadMembers();
      },
      error: (err) => this.notify.error(err?.error?.message || 'Signature upload failed.')
    });
  }

  saveMember() {
    const member = this.selectedMember();
    if (!member) return;

    if (!member.fullName || !member.mobileNo || !member.email) {
      this.notify.error('Member profile requires at least a Name, Mobile, and Email.');
      return;
    }

    const mobilePattern = /^01[3-9]\d{8}$/;
    if (!mobilePattern.test(member.mobileNo)) {
      this.notify.error('Please enter a valid 11-digit Bangladeshi mobile number.');
      return;
    }

    ensureValidAcademicData(member);

    this.submitting.set(true);
    this.adminService.updateMember(member.id, {
      fullName: member.fullName,
      status: member.status,
      fatherName: member.fatherName,
      motherName: member.motherName,
      dateOfBirth: toWireDate(member.dateOfBirth),
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
      membershipNumber: member.membershipNumber,
      membershipChangeReason: member.membershipChangeReason,
      isMobilePublic: member.isMobilePublic,
      isEmailPublic: member.isEmailPublic,
      isAddressPublic: member.isAddressPublic,
      isNIDPublic: member.isNIDPublic,
      isFamilyPublic: member.isFamilyPublic,
      isVerified: member.isVerified,
      contributionPoints: member.contributionPoints,
      notifyEventCreation: member.notifyEventCreation,
      notifyParticipationApproval: member.notifyParticipationApproval,
      notifyRegistrationUpdate: member.notifyRegistrationUpdate,
      notifyRelevantUpdates: member.notifyRelevantUpdates,
      tShirtSize: member.tShirtSize,
      emergencyContactName: member.emergencyContactName,
      emergencyContactRelation: member.emergencyContactRelation,
      emergencyContactPhone: member.emergencyContactPhone,
      academicHistory: member.academicHistory,
      professionalHistory: (member.professionalHistory || []).map((ph: any) => ({
        ...ph,
        startDate: toWireDate(ph.startDate),
        endDate: toWireDate(ph.endDate)
      })),
      ecHistory: (member.ecHistory || []).map((h: any) => ({
        ...h,
        startDate: toWireDate(h.startDate),
        endDate: toWireDate(h.endDate)
      })),
      ecChangeReason: member.ecChangeReason
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
      error: () => {
        this.notify.error('Update failed.');
        this.submitting.set(false);
      }
    });
  }

  private finalizeSave() {
    this.submitting.set(false);
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
    const file: File = event.target.files[0];
    if (!file) return;
    const err = validateUploadFile(file, 'pdf');
    if (err) { this.notify.error(err); event.target.value = ''; return; }
    if (type === 'cert') this.certToUpload = file;
    else this.payToUpload = file;
  }

  contactMember(email: string) {
    this.router.navigate(['/admin/comm'], { queryParams: { target: email, method: 'custom' } });
  }

  getStatusLabel = getStatusLabel;
  getStatusClass = getStatusClass;
  getCategoryLabel = getCategoryLabel;
  getMembershipTypeLabel = getMembershipTypeLabel;
  getECPositionName = getECPositionName;
  
  getCurrentPosition(member: any) {
    const pos = getCurrentECPosition(member.ecHistory);
    return getECPositionName(pos);
  }

  getCurrentPeriod(member: any) {
    return getCurrentECPeriod(member.ecHistory);
  }
  
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
    this.selectedMember.update(m => {
      if (!m.academicHistory) m.academicHistory = [];
      m.academicHistory.unshift({
        institutionName: '',
        degree: '',
        subject: '',
        admissionYear: new Date().getFullYear() - 4,
        passingYear: new Date().getFullYear(),
        isGHC: false,
        result: ''
      });
      return { ...m };
    });
  }

  removeAcademic(index: number) {
    this.selectedMember.update(m => {
      m.academicHistory.splice(index, 1);
      return { ...m };
    });
  }

  addProfessional() {
    this.selectedMember.update(m => {
      if (!m.professionalHistory) m.professionalHistory = [];
      m.professionalHistory.unshift({
        organizationName: '',
        designation: '',
        sector: '',
        location: '',
        startDate: this.datePipe.transform(new Date(), 'dd-MM-yyyy') || '',
        isCurrent: true
      });
      return { ...m };
    });
  }

  removeProfessional(index: number) {
    this.selectedMember.update(m => {
      m.professionalHistory.splice(index, 1);
      return { ...m };
    });
  }

  addECHistory() {
    if (!this.selectedMember().ecHistory) this.selectedMember().ecHistory = [];
    const activePeriod = this.ecPeriods().find(p => p.isActive);
    this.selectedMember().ecHistory.unshift({
      periodTitle: activePeriod ? activePeriod.title : '',
      position: 0, // None
      startDate: this.datePipe.transform(new Date(), 'dd-MM-yyyy') || '',
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

  getLabel(options: any[], value: any): string {
    if (value === null || value === undefined) return 'Not Specified';
    // Stringify comparison to handle string vs number (e.g. "Male" vs "Male", or enum 0 vs "0")
    // Special case for enums: if value is a number, we might need to match its string representation if options use that
    const option = options.find(o => String(o.value).toLowerCase() === String(value).toLowerCase());
    return option ? option.label : String(value);
  }
}
