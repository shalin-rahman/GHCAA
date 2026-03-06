import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { ACADEMIC_CERTIFICATES, ACADEMIC_GROUPS, ACADEMIC_SUBJECTS, PROFESSIONAL_SECTORS, getAcademicYears } from '../../core/constants/app.constants';

@Component({
  selector: 'app-member-approval',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-approval.html',
  styleUrl: './member-approval.scss'
})
export class MemberApproval implements OnInit {
  private adminService = inject(AdminService);
  private auth = inject(AuthService);
  private router = inject(Router);

  requests = signal<any[]>([]);
  loading = signal(true);
  selectedMember = signal<any | null>(null);
  years = getAcademicYears();
  certificateOptions = ACADEMIC_CERTIFICATES;
  groupOptions = ACADEMIC_GROUPS;
  subjectOptions = ACADEMIC_SUBJECTS;
  sectorOptions = PROFESSIONAL_SECTORS;

  rejecting = signal(false);
  rejectionReason = '';

  ngOnInit() {
    this.loadMembers();
  }

  loadMembers() {
    this.loading.set(true);
    this.adminService.getPendingMembers().subscribe({
      next: (data) => {
        // Only show applied status (0) in logic if preferred, 
        // though backend might already filter for pending
        this.requests.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  viewDetails(member: any) {
    this.selectedMember.set(member);
    this.rejecting.set(false);
    this.rejectionReason = '';
  }

  closeAudit() {
    this.selectedMember.set(null);
    this.rejecting.set(false);
  }

  approve(id: number) {
    if (confirm('Verify this registry entry? This will officially induct the member and dispatch credentials.')) {
      const adminId = this.auth.currentUser()?.memberId || 1;
      this.adminService.approveMember(id, adminId).subscribe({
        next: () => {
          alert('Registry verified. Member successfully inducted.');
          this.selectedMember.set(null);
          this.loadMembers();
        }
      });
    }
  }

  confirmReject() {
    if (!this.rejectionReason) return;
    if (confirm('Permanently decline this registry filing? The applicant will be notified with your reason.')) {
      const adminId = this.auth.currentUser()?.memberId || 1;
      this.adminService.rejectMember(this.selectedMember().id, adminId, this.rejectionReason).subscribe({
        next: () => {
          alert('Application declined. Record removed from active queue.');
          this.selectedMember.set(null);
          this.loadMembers();
        }
      });
    }
  }

  contactMember(email: string) {
    this.router.navigate(['/admin/comm'], { queryParams: { target: email, method: 'custom' } });
  }

  getStatusName(status: any): string {
    const map: Record<string, string> = {
      'Applied': 'Pending Audit',
      'Active': 'Active',
      'InactivePayment': 'Inactive',
      'InactiveResigned': 'Resigned',
      'Terminated': 'Terminated'
    };
    return map[String(status)] || 'Unknown';
  }

  getStatusClass(status: any): string {
    const map: Record<string, string> = {
      'Applied': 'pending',
      'Active': 'active',
      'InactivePayment': 'inactive',
      'InactiveResigned': 'inactive',
      'Terminated': 'terminated'
    };
    return map[String(status)] || '';
  }
}
