import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { Router, ActivatedRoute } from '@angular/router';
import { EC_ROLES, getECPositionName, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData } from '../../core/constants/app.constants';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-admin-members',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
  searchQuery = signal('');
  statusFilter = signal('all');
  selectedMember = signal<any>(null);
  isEditing = signal(false);

  // Import State
  showImportModal = signal(false);
  isImporting = signal(false);
  importFile: File | null = null;
  photoFiles: File[] = [];
  excelHeaders: string[] = [];
  columnMapping: Record<string, string> = {
    'Full Name': 'FullName',
    'Email': 'Email',
    'Mobile': 'MobileNo',
    'NID': 'NID',
    'Batch': 'GHCLastCertificatePassingYear',
    'ID': 'ID' // For photo matching
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
    { value: 'ECPosition', label: 'EC Position' },
    // System
    { value: 'ID', label: 'System/External ID (For Photos)' }
  ];

  // Constants for dropdowns
  membershipTypes = [
    { value: 'Founding', label: 'Founding Member' },
    { value: 'Executive', label: 'Executive Member' },
    { value: 'General', label: 'General Member' },
    { value: 'Associate', label: 'Associate Member' },
    { value: 'Honorary', label: 'Honorary Member' },
    { value: 'Advisory', label: 'Advisory Member' }
  ];

  memberCategories = [
    { value: 'None', label: 'No Special status' },
    { value: 'Lifelong', label: 'Lifelong Member' },
    { value: 'Donor', label: 'Donor Member' },
    { value: 'Patron', label: 'Patron Member' }
  ];

  ecPositions = EC_ROLES.map((label, index) => ({ value: index, label }));

  ACADEMIC = ACADEMIC_DATA;
  yearsList = this.ACADEMIC.getYears();
  IS_HSC = IS_HSC;
  degreeOptions = this.ACADEMIC.certificates;
  groupOptions = this.ACADEMIC.groups;
  subjectOptions = this.ACADEMIC.subjects;
  sectorOptions = this.ACADEMIC.sectors;
  filteredMembers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.statusFilter();
    return this.allMembers().filter(m => {
      const matchQ = !q ||
        m.fullName?.toLowerCase().includes(q) ||
        m.email?.toLowerCase().includes(q) ||
        m.membershipNumber?.toLowerCase().includes(q);
      const matchS = s === 'all' || String(m.status) === s;
      return matchQ && matchS;
    });
  });

  statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: 'Applied', label: 'Applied' },
    { value: 'Active', label: 'Active' },
    { value: 'InactivePayment', label: 'Inactive (Payment)' },
    { value: 'InactiveResigned', label: 'Inactive (Resigned)' },
  ];

  ngOnInit() {
    this.loadMembers();
  }

  loadMembers() {
    this.loading.set(true);
    this.adminService.getMembers().subscribe({
      next: (data: any[]) => { this.allMembers.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
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

  openDetail(member: any) { this.selectedMember.set({ ...member }); this.isEditing.set(false); }
  closeDetail() { this.selectedMember.set(null); this.isEditing.set(false); }

  toggleEdit() { this.isEditing.update(v => !v); }

  saveMember() {
    const member = this.selectedMember();
    if (!member) return;

    ensureValidAcademicData(member);

    this.adminService.updateMember(member.id, {
      fullName: member.fullName,
      fatherName: member.fatherName,
      motherName: member.motherName,
      dateOfBirth: member.dateOfBirth,
      nid: member.nid,
      mobileNo: member.mobileNo,
      email: member.email,
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
      isAddressPublic: member.isAddressPublic
    }).subscribe({
      next: () => {
        this.notify.success('Member information updated.');
        this.isEditing.set(false);
        this.loadMembers();
      },
      error: () => this.notify.error('Update failed.')
    });
  }

  contactMember(email: string) {
    this.router.navigate(['/admin/comm'], { queryParams: { target: email, method: 'custom' } });
  }

  getStatusLabel(status: any): string {
    return this.statusOptions.find(s => s.value === String(status))?.label ?? 'Unknown';
  }

  getStatusClass(status: any): string {
    const map: Record<string, string> = {
      'Applied': 'pending',
      'Active': 'active',
      'InactivePayment': 'inactive',
      'InactiveResigned': 'resigned'
    };
    return map[String(status)] ?? '';
  }

  getCategoryLabel(cat: any): string {
    const cats = ['None', 'Lifelong', 'Donor', 'Patron'];
    if (typeof cat === 'number') return cats[cat] || 'None';
    return cat || 'None';
  }

  getECPositionLabel(pos: any): string {
    return getECPositionName(pos);
  }

  // --- Import Actions ---
  openImport() {
    this.showImportModal.set(true);
    this.importFile = null;
    this.photoFiles = [];
    this.excelHeaders = [];
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.importFile = file;
      this.extractHeaders(file);
    }
  }

  onPhotosSelected(event: any) {
    this.photoFiles = Array.from(event.target.files);
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
          this.excelHeaders = (jsonData[0] as any[]).filter((h: any) => h != null && String(h).trim() !== '').map((h: any) => String(h).trim());

          // Reset mappings
          this.columnMapping = {};

          // Auto-map by fuzzy matching
          this.excelHeaders.forEach(h => {
            const lower = h.toLowerCase();
            if ((lower.includes('full') && lower.includes('name')) || lower === 'name') this.columnMapping['FullName'] = h;
            else if (lower.includes('father')) this.columnMapping['FatherName'] = h;
            else if (lower.includes('mother')) this.columnMapping['MotherName'] = h;
            else if (lower.includes('email') || lower.includes('e-mail')) this.columnMapping['Email'] = h;
            else if (lower.includes('mobile') || lower.includes('phone') || lower.includes('cell')) this.columnMapping['MobileNo'] = h;
            else if (lower === 'nid' || lower.includes('national id')) this.columnMapping['NID'] = h;
            else if (lower.includes('batch') || lower.includes('passing year') || lower.includes('session')) this.columnMapping['GHCLastCertificatePassingYear'] = h;
            else if (lower.includes('designation') || lower.includes('position')) this.columnMapping['Designation'] = h;
            else if (lower.includes('sector') || lower.includes('profession')) this.columnMapping['ProfessionalSector'] = h;
            else if (lower.includes('blood')) this.columnMapping['BloodGroup'] = h;
            else if (lower.includes('gender') || lower.includes('sex')) this.columnMapping['Gender'] = h;
            else if (lower.includes('dob') || lower.includes('birth')) this.columnMapping['DateOfBirth'] = h;
            else if (lower.includes('present') && lower.includes('address')) this.columnMapping['PresentAddress'] = h;
            else if (lower.includes('permanent') && lower.includes('address')) this.columnMapping['PermanentAddress'] = h;
            else if (lower.includes('address') && !lower.includes('present') && !lower.includes('permanent')) this.columnMapping['PresentAddress'] = h;
            else if (lower === 'id' || lower === 'sl' || lower === 'serial') this.columnMapping['ID'] = h;
            else if (lower.includes('membership') && lower.includes('no')) this.columnMapping['MembershipNumber'] = h;
            else if (lower.includes('membership') && lower.includes('type')) this.columnMapping['MembershipType'] = h;
            else if (lower.includes('certificate') && lower.includes('highest')) this.columnMapping['HighestCertificate'] = h;
            else if (lower.includes('certificate') && lower.includes('ghc')) this.columnMapping['GHCLastCertificate'] = h;
            else if (lower.includes('emergency') && lower.includes('name')) this.columnMapping['EmergencyContactName'] = h;
            else if (lower.includes('emergency') && lower.includes('relation')) this.columnMapping['EmergencyContactRelation'] = h;
            else if (lower.includes('emergency') && lower.includes('phone')) this.columnMapping['EmergencyContactPhone'] = h;
            else if (lower.includes('hsc') && lower.includes('admission')) this.columnMapping['HSCAdmissionYear'] = h;
            else if (lower.includes('ghc') && lower.includes('admission')) this.columnMapping['GHCAdmissionYear'] = h;
          });

          this.notify.success(`Found ${this.excelHeaders.length} columns in the Excel file.`);
        } else {
          this.notify.warning('The Excel file appears to be empty.');
          this.excelHeaders = [];
        }
      } catch (err) {
        console.error('Excel parse error:', err);
        this.notify.error('Failed to read Excel file. Please ensure it is a valid .xlsx file.');
        this.excelHeaders = [];
      }
    };
    reader.readAsArrayBuffer(file);
  }

  executeImport() {
    if (!this.importFile) return;

    this.isImporting.set(true);
    const formData = new FormData();
    formData.append('ExcelFile', this.importFile);
    this.photoFiles.forEach(f => formData.append('Photos', f));

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
          this.notify.warning(`${res.failureCount} rows failed. See console for details.`);
          console.error('Import Errors:', res.errors);
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
}
