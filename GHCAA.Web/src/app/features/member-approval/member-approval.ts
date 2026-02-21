import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-member-approval',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-feature">
      <div class="header">
        <div class="title-group">
          <h2>Registry Audit Desk</h2>
          <p>Review and adjudicate incoming membership applications.</p>
        </div>
        <div class="stats">
          <div class="stat-badge">Pending Verification: {{ requests().length }}</div>
        </div>
      </div>

      <div class="glass-card table-container">
        @if (loading()) {
            <div class="loading-state">
              <div class="spinner"></div>
              <p>Fetching Registry data...</p>
            </div>
        } @else {
            <table class="admin-table">
            <thead>
                <tr>
                <th>Applicant Profile</th>
                <th>Academic Credentials</th>
                <th>Status</th>
                <th class="text-right">Operations</th>
                </tr>
            </thead>
            <tbody>
                @for (reg of requests(); track reg.id) {
                 @if (reg.status === 0) {
                  <tr [class.viewing]="selectedMember()?.id === reg.id">
                      <td>
                        <div class="member-cell">
                            <div class="avatar-box">
                              @if (reg.photoPath) {
                                <img [src]="reg.photoPath" alt="Profile">
                              } @else {
                                <div class="avatar">{{ reg.fullName[0] }}</div>
                              }
                            </div>
                            <div class="info">
                              <strong>{{ reg.fullName }}</strong>
                              <small>{{ reg.email }}</small>
                            </div>
                        </div>
                      </td>
                      <td>
                        <div class="academic-info">
                          <strong>{{ reg.subjectGroup }}</strong>
                          <small>Class of {{ reg.ghcLastCertificatePassingYear }}</small>
                        </div>
                      </td>
                      <td>
                        <span class="badge" [ngClass]="getStatusClass(reg.status)">
                            {{ getStatusName(reg.status) }}
                        </span>
                      </td>
                      <td>
                        <div class="action-btns">
                          <button (click)="viewDetails(reg)" class="btn btn-secondary btn-sm">Audit Details</button>
                          <button (click)="approve(reg.id)" class="btn btn-primary btn-sm">Direct Verify</button>
                        </div>
                      </td>
                  </tr>
                 }
                }
            </tbody>
            </table>
        }
      </div>

      <!-- Details Modal -->
      <div class="modal-overlay" *ngIf="selectedMember()" (click)="closeAudit()">
        <div class="modal-content glass-card reveal-top" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="header-info">
              <h3>Applicant Registry File: {{ selectedMember()?.fullName }}</h3>
              <span class="badge pending">Audit in Progress</span>
            </div>
            <button class="close-btn" (click)="closeAudit()">×</button>
          </div>
          
          <div class="modal-body audit-tabs">
            <div class="tab-content audit-grid" *ngIf="!rejecting()">
              <div class="audit-section">
                <h4>Identity & Profile</h4>
                <div class="profile-preview">
                  <img *ngIf="selectedMember()?.photoPath" [src]="selectedMember()?.photoPath" class="audit-photo">
                  <div class="profile-details">
                    <div class="data-row"><span>Father's Name:</span> <strong>{{ selectedMember()?.fatherName }}</strong></div>
                    <div class="data-row"><span>Mother's Name:</span> <strong>{{ selectedMember()?.motherName }}</strong></div>
                    <div class="data-row"><span>Date of Birth:</span> <strong>{{ selectedMember()?.dateOfBirth | date }}</strong></div>
                    <div class="data-row"><span>NID / Identity:</span> <strong>{{ selectedMember()?.nid }}</strong></div>
                    <div class="data-row"><span>Blood Group:</span> <strong>{{ selectedMember()?.bloodGroup }}</strong></div>
                  </div>
                </div>
              </div>

              <div class="audit-section">
                <h4>Academic Verification</h4>
                <div class="data-row"><span>Subject / Group:</span> <strong>{{ selectedMember()?.subjectGroup }}</strong></div>
                <div class="data-row"><span>Degree at GHC:</span> <strong>{{ selectedMember()?.lastCertificateFromGHC }}</strong></div>
                <div class="data-row"><span>Passing Year:</span> <strong>{{ selectedMember()?.ghcLastCertificatePassingYear }}</strong></div>
                <div class="data-row"><span>HSC Adm. Year:</span> <strong>{{ selectedMember()?.hscAdmissionYear }}</strong></div>
                
                <div class="doc-link" *ngIf="selectedMember()?.certificatePath">
                  <a [href]="selectedMember()?.certificatePath" target="_blank" class="btn btn-sm btn-secondary w-full">Inspect Certificate Copy ↗</a>
                </div>
              </div>

              <div class="audit-section">
                <h4>Vocational Details</h4>
                <div class="data-row"><span>Sector:</span> <strong>{{ selectedMember()?.professionalSector }}</strong></div>
                <div class="data-row"><span>Designation:</span> <strong>{{ selectedMember()?.designation }}</strong></div>
              </div>

              <div class="audit-section">
                <h4>Emergency Contact</h4>
                <div class="data-row"><span>Contact Name:</span> <strong>{{ selectedMember()?.emergencyContactName }}</strong></div>
                <div class="data-row"><span>Mobile:</span> <strong>{{ selectedMember()?.emergencyContactPhone }}</strong></div>
              </div>
            </div>

            <div class="rejection-form reveal-text" *ngIf="rejecting()">
               <h4>Application Rejection</h4>
               <p>Provide a clear reason for rejecting this registry filing. This will be transmitted to the applicant.</p>
               <textarea [(ngModel)]="rejectionReason" rows="5" placeholder="Reason for rejection (e.g. Invalid certificate copy, NID mismatch)..."></textarea>
               <div class="reject-actions">
                 <button class="btn btn-secondary" (click)="rejecting.set(false)">Back to Audit</button>
                 <button class="btn btn-danger" (click)="confirmReject()" [disabled]="!rejectionReason">Execute Rejection</button>
               </div>
            </div>
          </div>

          <div class="modal-footer" *ngIf="!rejecting()">
            <button class="btn btn-danger-outline" (click)="rejecting.set(true)">Decline Filing</button>
            <div class="right-btns">
              <button class="btn btn-secondary" (click)="closeAudit()">Suspend</button>
              <button class="btn btn-primary" (click)="approve(selectedMember()!.id)">Verify & Approve</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .header { margin-bottom: 2.5rem; display: flex; justify-content: space-between; align-items: flex-end; }
    .header h2 { font-size: 1.75rem; color: var(--primary-color); font-weight: 800; letter-spacing: -0.5px; }
    .header p { color: var(--text-muted); font-size: 0.9rem; margin-top: 0.25rem; }
    .stat-badge { background: var(--primary-color); color: white; padding: 0.5rem 1rem; border-radius: 8px; font-weight: 700; font-size: 0.8rem; }
    
    .table-container { overflow-x: auto; border: 1px solid var(--glass-border); background: var(--surface-color); }
    .admin-table { width: 100%; border-collapse: collapse; }
    .admin-table th { padding: 1.25rem 1.5rem; background: rgba(0,0,0,0.02); color: var(--text-muted); font-size: 0.75rem; text-transform: uppercase; font-weight: 800; text-align: left; }
    .admin-table td { padding: 1.25rem 1.5rem; border-bottom: 1px solid var(--glass-border); }
    .admin-table tr:last-child td { border: none; }
    .admin-table tr.viewing { background: rgba(212, 175, 55, 0.05); }

    .member-cell { display: flex; align-items: center; gap: 1rem; }
    .avatar-box { width: 45px; height: 45px; border-radius: 8px; overflow: hidden; background: var(--bg-color); border: 1px solid var(--border-color); }
    .avatar-box img { width: 100%; height: 100%; object-fit: cover; }
    .avatar { width: 100%; height: 100%; background: var(--primary-color); color: white; display: flex; align-items: center; justify-content: center; font-weight: 800; }
    
    .info { display: flex; flex-direction: column; gap: 2px; }
    .info strong { font-size: 0.95rem; color: var(--text-main); }
    .info small { color: var(--text-muted); font-size: 0.8rem; }
    
    .academic-info { display: flex; flex-direction: column; gap: 2px; }
    .academic-info strong { font-size: 0.9rem; color: var(--primary-color); }
    .academic-info small { font-size: 0.8rem; color: var(--text-muted); }

    .badge { padding: 0.35rem 0.85rem; border-radius: 6px; font-size: 0.7rem; font-weight: 800; text-transform: uppercase; }
    .badge.pending { background: rgba(212, 175, 55, 0.1); color: var(--accent-color); border: 1px solid var(--accent-color); }
    .badge.active { background: #E0F2F1; color: #00796B; }

    .action-btns { display: flex; gap: 0.75rem; justify-content: flex-end; }
    .text-right { text-align: right; }

    /* Modal Styling */
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.7); backdrop-filter: blur(8px); z-index: 2000; display: flex; align-items: center; justify-content: center; padding: 2rem; }
    .modal-content { width: 100%; max-width: 900px; max-height: 90vh; display: flex; flex-direction: column; border: 1px solid var(--accent-color); }
    .modal-header { padding: 2rem; border-bottom: 1px solid var(--glass-border); display: flex; justify-content: space-between; align-items: center; background: var(--bg-color); }
    .modal-header h3 { font-size: 1.25rem; font-weight: 800; color: var(--primary-color); }
    .close-btn { font-size: 2.5rem; border: none; background: none; color: var(--text-muted); cursor: pointer; transition: 0.3s; }
    .close-btn:hover { color: var(--accent-color); }

    .audit-grid { padding: 2.5rem; display: grid; grid-template-columns: 1fr 1fr; gap: 3rem; overflow-y: auto; background: var(--bg-color); }
    .audit-section h4 { font-size: 0.75rem; text-transform: uppercase; color: var(--accent-color); margin-bottom: 1.5rem; border-bottom: 1px solid var(--glass-border); padding-bottom: 0.75rem; letter-spacing: 1px; font-weight: 800; }
    
    .profile-preview { display: flex; gap: 2rem; }
    .audit-photo { width: 110px; height: 140px; border-radius: 12px; object-fit: cover; border: 2px solid var(--accent-color); box-shadow: var(--shadow-md); flex-shrink: 0; }
    .profile-details { flex-grow: 1; }

    .data-row { display: flex; justify-content: space-between; font-size: 0.9rem; padding: 0.6rem 0; border-bottom: 1px dotted var(--glass-border); }
    .data-row span { color: var(--text-muted); font-weight: 500; }
    .data-row strong { color: var(--text-main); font-weight: 700; }

    .doc-link { margin-top: 1.5rem; }

    .modal-footer { padding: 1.5rem 2.5rem; border-top: 1px solid var(--glass-border); display: flex; justify-content: space-between; gap: 1rem; background: var(--surface-color); }
    .right-btns { display: flex; gap: 1rem; }

    .rejection-form { padding: 3rem; background: var(--bg-color); height: 100%; }
    .rejection-form h4 { color: #d32f2f; margin-bottom: 1rem; font-weight: 800; font-size: 1.25rem; }
    .rejection-form p { color: var(--text-muted); margin-bottom: 2rem; font-size: 0.95rem; line-height: 1.6; }
    .rejection-form textarea { width: 100%; padding: 1.5rem; border-radius: 12px; border: 2px solid var(--border-color); background: var(--bg-color); color: var(--text-main); font-size: 1rem; margin-bottom: 2rem; }
    .rejection-form textarea:focus { border-color: #d32f2f; outline: none; }
    .reject-actions { display: flex; gap: 1rem; justify-content: flex-end; }

    .btn-danger-outline { background: transparent; border: 1px solid #d32f2f; color: #d32f2f; font-weight: 700; }
    .btn-danger-outline:hover { background: #d32f2f; color: white; }
    .btn-danger { background: #d32f2f; color: white; font-weight: 800; }
  `]
})
export class MemberApproval implements OnInit {
  private adminService = inject(AdminService);
  private auth = inject(AuthService);

  requests = signal<any[]>([]);
  loading = signal(true);
  selectedMember = signal<any | null>(null);

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

  getStatusName(status: any): string {
    const statuses = ['Pending Audit', 'Active', 'Inactive', 'Resigned', 'Terminated'];
    return statuses[status] || 'Unknown';
  }

  getStatusClass(status: any): string {
    const classes = ['pending', 'active', 'inactive', 'inactive', 'terminated'];
    return classes[status] || '';
  }
}
