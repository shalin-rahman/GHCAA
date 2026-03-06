import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';
import { Router, ActivatedRoute } from '@angular/router';
import { EC_ROLES, getECPositionName, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData } from '../../core/constants/app.constants';

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
    { value: 'FullName', label: 'Full Name' },
    { value: 'Email', label: 'Email Address' },
    { value: 'MobileNo', label: 'Mobile Number' },
    { value: 'NID', label: 'NID Number' },
    { value: 'FatherName', label: "Father's Name" },
    { value: 'MotherName', label: "Mother's Name" },
    { value: 'DateOfBirth', label: 'Date of Birth' },
    { value: 'Gender', label: 'Gender' },
    { value: 'BloodGroup', label: 'Blood Group' },
    { value: 'GHCLastCertificatePassingYear', label: 'Passing Year' },
    { value: 'MembershipNumber', label: 'Membership No.' },
    { value: 'Designation', label: 'Professional Designation' },
    { value: 'ProfessionalSector', label: 'Sector' },
    { value: 'PresentAddress', label: 'Present Address' },
    { value: 'PermanentAddress', label: 'Permanent Address' },
    { value: 'HighestCertificate', label: 'Highest Certificate' },
    { value: 'GHCLastCertificate', label: 'GHC Last Certificate' },
    { value: 'HSCAdmissionYear', label: 'HSC Admission Year' },
    { value: 'GHCAdmissionYear', label: 'GHC Admission Year' },
    { value: 'EmergencyContactName', label: 'Emergency Contact Name' },
    { value: 'EmergencyContactRelation', label: 'Emergency Contact Relation' },
    { value: 'EmergencyContactPhone', label: 'Emergency Contact Phone' },
    { value: 'MembershipType', label: 'Membership Type' },
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
    // For production, usually we'd use a lib like 'xlsx', but here we rely on standard headers 
    // and manual mapping.
    this.excelHeaders = ['ID', 'Full Name', 'Email', 'Mobile', 'NID', 'Batch', 'Father Name', 'Mother Name', 'Designation'];

    // Auto-map based on common strings
    this.excelHeaders.forEach(h => {
      const lower = h.toLowerCase();
      if (lower.includes('name') && !lower.includes('father') && !lower.includes('mother')) this.columnMapping[h] = 'FullName';
      if (lower.includes('email')) this.columnMapping[h] = 'Email';
      if (lower.includes('mobile') || lower.includes('phone')) this.columnMapping[h] = 'MobileNo';
      if (lower.includes('id')) this.columnMapping[h] = 'ID';
    });
  }

  executeImport() {
    if (!this.importFile) return;

    this.isImporting.set(true);
    const formData = new FormData();
    formData.append('ExcelFile', this.importFile);
    this.photoFiles.forEach(f => formData.append('Photos', f));
    formData.append('ColumnMappingJson', JSON.stringify(this.columnMapping));
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
