import { Component, signal, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { ACADEMIC_CERTIFICATES, ACADEMIC_SUBJECTS, PROFESSIONAL_SECTORS, getAcademicYears, getStatusLabel, getStatusClass } from '../../core/constants/app.constants';

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
  private notify = inject(NotificationService);

  requests = signal<any[]>([]);
  pendingRequests = computed(() => {
    return this.requests().filter(r => r.status === 'Applied' || String(r.status) === '0' || r.status === 0);
  });
  loading = signal(true);
  selectedMember = signal<any | null>(null);
  searchQuery = signal('');

  filteredRequests = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.pendingRequests();
    return this.pendingRequests().filter(r =>
      (r.fullName || '').toLowerCase().includes(q) ||
      (r.email || '').toLowerCase().includes(q) ||
      (r.membershipNumber || '').toLowerCase().includes(q)
    );
  });
  years = getAcademicYears();
  certificateOptions = ACADEMIC_CERTIFICATES;
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
        this.requests.set(data.items || []);
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
          this.notify.success('Registry verified. Member successfully inducted.');
          this.selectedMember.set(null);
          this.loadMembers();
        },
        error: () => this.notify.error('Failed to verify registry.')
      });
    }
  }

  confirmReject() {
    if (!this.rejectionReason) return;
    if (confirm('Permanently decline this registry filing? The applicant will be notified with your reason.')) {
      const adminId = this.auth.currentUser()?.memberId || 1;
      this.adminService.rejectMember(this.selectedMember().id, adminId, this.rejectionReason).subscribe({
        next: () => {
          this.notify.success('Application declined. Record removed from active queue.');
          this.selectedMember.set(null);
          this.loadMembers();
        },
        error: () => this.notify.error('Failed to decline application.')
      });
    }
  }

  contactMember(email: string) {
    this.router.navigate(['/admin/comm'], { queryParams: { target: email, method: 'custom' } });
  }

  getStatusName(status: any): string {
    return getStatusLabel(status);
  }

  getStatusClass(status: any): string {
    return getStatusClass(status);
  }
}


