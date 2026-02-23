import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { NavService } from '../../core/services/nav.service';

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
  nav = inject(NavService);

  allMembers = signal<any[]>([]);
  loading = signal(true);
  searchQuery = signal('');
  statusFilter = signal('all');
  selectedMember = signal<any>(null);

  filteredMembers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.statusFilter();
    return this.allMembers().filter(m => {
      const matchQ = !q ||
        m.fullName?.toLowerCase().includes(q) ||
        m.email?.toLowerCase().includes(q) ||
        m.membershipNumber?.toLowerCase().includes(q) ||
        m.ghcLastCertificatePassingYear?.toString().includes(q);
      const matchS = s === 'all' || String(m.status) === s;
      return matchQ && matchS;
    });
  });

  statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: '0', label: 'Applied' },
    { value: '1', label: 'Active' },
    { value: '2', label: 'Inactive (Payment)' },
    { value: '3', label: 'Inactive (Resigned)' },
  ];

  ngOnInit() { this.loadMembers(); }

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

  openDetail(member: any) { this.selectedMember.set(member); }
  closeDetail() { this.selectedMember.set(null); }

  getStatusLabel(status: any): string {
    return this.statusOptions.find(s => s.value === String(status))?.label ?? 'Unknown';
  }

  getStatusClass(status: any): string {
    const map: Record<string, string> = { '0': 'pending', '1': 'active', '2': 'inactive', '3': 'resigned' };
    return map[String(status)] ?? '';
  }
}
