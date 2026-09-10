import { Component, signal, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { AdminService } from '../../core/services/admin.service';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ACADEMIC_CERTIFICATES, ACADEMIC_SUBJECTS, PROFESSIONAL_SECTORS, getStatusLabel, getStatusClass } from '../../core/constants/app.constants';
import { LookupService } from '../../core/services/lookup.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

@Component({
  selector: 'app-member-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective],
  templateUrl: './member-approval.html',
  styleUrl: './member-approval.scss'
})
export class MemberApproval implements OnInit {
  private adminService = inject(AdminService);
  private router = inject(Router);
  private notify = inject(NotificationService);
  private lookupService = inject(LookupService);
  private confirmDialog = inject(ConfirmDialogService);

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
  // 82.42: sourced from /lookups/PassingYear via LookupService, filled in ngOnInit.
  years: number[] = [];
  certificateOptions = ACADEMIC_CERTIFICATES;
  subjectOptions = ACADEMIC_SUBJECTS;
  sectorOptions = PROFESSIONAL_SECTORS;

  rejecting = signal(false);
  rejectionReason = '';
  processing = signal(false);

  ngOnInit() {
    this.loadMembers();
    this.lookupService.getAcademicYears().subscribe(years => this.years = years);
  }

  loadMembers() {
    this.loading.set(true);
    this.adminService.getPendingMembers().subscribe({
      next: (data) => {
        // 32.3: generic case-insensitive key normalization, was a hardcoded field whitelist
        const items = (data.items || []).map((obj: any) => {
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
        });
        
        this.requests.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  detailLoading = signal(false);

  viewDetails(member: any) {
    // 29D.2: The list row only carries 8 summary fields, but the audit panel binds
    // ~15 (father/mother name, DOB, NID, addresses, academic/professional history,
    // certificatePath). Show the summary immediately, then fetch the full record so
    // those sections populate instead of rendering blank.
    this.selectedMember.set(member);
    this.rejecting.set(false);
    this.rejectionReason = '';
    this.detailLoading.set(true);
    this.adminService.getMemberById(member.id).subscribe({
      next: (full) => {
        // Guard against a stale response if the admin closed/switched panels meanwhile.
        if (this.selectedMember()?.id === member.id) {
          this.selectedMember.set({ ...member, ...full });
        }
        this.detailLoading.set(false);
      },
      error: () => {
        this.detailLoading.set(false);
        this.notify.error('Failed to load full applicant details.');
      }
    });
  }

  closeAudit() {
    this.selectedMember.set(null);
    this.rejecting.set(false);
  }

  async approve(id: number) {
    if (this.processing()) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Verify registry entry',
      message: 'Verify this registry entry? This will officially induct the member and dispatch credentials.',
      confirmLabel: 'Verify'
    }));
    if (!ok) return;

    this.processing.set(true);
    this.adminService.approveMember(id).subscribe({
      next: () => {
        this.processing.set(false);
        this.notify.success('Registry verified. Member successfully inducted.');
        this.selectedMember.set(null);
        this.loadMembers();
      },
      error: () => {
        this.processing.set(false);
        this.notify.error('Failed to verify registry.');
      }
    });
  }

  async confirmReject() {
    if (this.processing() || !this.rejectionReason) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Decline registry filing',
      message: 'Permanently decline this registry filing? The applicant will be notified with your reason.',
      confirmLabel: 'Decline',
      danger: true
    }));
    if (!ok) return;

    this.processing.set(true);
    this.adminService.rejectMember(this.selectedMember().id, this.rejectionReason).subscribe({
      next: () => {
        this.processing.set(false);
        this.notify.success('Application declined. Record removed from active queue.');
        this.selectedMember.set(null);
        this.loadMembers();
      },
      error: () => {
        this.processing.set(false);
        this.notify.error('Failed to decline application.');
      }
    });
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

