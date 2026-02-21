import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService, MemberProfile } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="profile-page">
      <div class="header">
        <h2>Member Dashboard</h2>
        <p>Manage your association identity and professional details</p>
      </div>

      @if (loading()) {
        <div class="glass-card loading-box">Syncing profile with registry...</div>
      } @else {
        <!-- Membership Card (Read Only) -->
        <div class="glass-card membership-status-card">
            <div class="status-header">
                <div class="main-info">
                    <span class="m-number">{{ profile.membershipNumber || 'PENDING APPROVAL' }}</span>
                    <h3>{{ profile.fullName }}</h3>
                    <p class="m-type">{{ getMembershipType(profile.membershipType) }} Member</p>
                </div>
                <div class="badge-icon" [ngClass]="profile.status === 1 ? 'active' : 'pending'">
                    {{ profile.status === 1 ? 'Verified' : 'Review' }}
                </div>
            </div>
            <div class="status-grid">
                <div class="grid-item">
                    <small>Batch/Passing Year</small>
                    <p>{{ profile.ghcLastCertificatePassingYear }}</p>
                </div>
                <div class="grid-item">
                    <small>Degree from GHC</small>
                    <p>{{ getDegreeName(profile.lastCertificateFromGHC) }}</p>
                </div>
                <div class="grid-item">
                    <small>Member Since</small>
                    <p>{{ (profile.approvedDate || profile.appliedDate) | date:'MMM yyyy' }}</p>
                </div>
            </div>
        </div>

        <div class="glass-card profile-card">
          <form (submit)="updateProfile()">
            <!-- Profession -->
            <div class="section">
                <h4>Professional Information</h4>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Professional Sector</label>
                        <select [(ngModel)]="profile.professionalSector" name="sector">
                            <option value="Govt. Service">Govt. Service</option>
                            <option value="Corporate">Corporate</option>
                            <option value="Business">Business</option>
                            <option value="Education">Education</option>
                            <option value="Medical">Medical/Health</option>
                            <option value="Engineering">Engineering</option>
                            <option value="Other">Other</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Current Designation</label>
                        <input type="text" [(ngModel)]="profile.designation" name="designation" placeholder="e.g. Senior Consultant">
                    </div>
                </div>
            </div>

            <!-- Contact Details -->
            <div class="section">
                <h4>Communication Address</h4>
                <div class="form-grid">
                    <div class="form-group full-width">
                        <label>Present Address</label>
                        <textarea [(ngModel)]="profile.presentAddress" name="presentAddress" rows="2"></textarea>
                    </div>
                </div>
            </div>

            <!-- Privacy Settings -->
            <div class="section privacy">
                <h4>Privacy & Visibility</h4>
                <p class="text-muted">Control your visibility in the HARAGANGIAN member directory</p>
                <div class="toggle-list">
                    <div class="toggle-item">
                        <label class="switch">
                            <input type="checkbox" [(ngModel)]="profile.isEmailPublic" name="isEmailPublic">
                            <span class="slider"></span>
                        </label>
                        <span>Display Email to other Alumni</span>
                    </div>
                    <div class="toggle-item">
                        <label class="switch">
                            <input type="checkbox" [(ngModel)]="profile.isMobilePublic" name="isMobilePublic">
                            <span class="slider"></span>
                        </label>
                        <span>Display Mobile Number to other Alumni</span>
                    </div>
                    <div class="toggle-item">
                        <label class="switch">
                            <input type="checkbox" [(ngModel)]="profile.isAddressPublic" name="isAddressPublic">
                            <span class="slider"></span>
                        </label>
                        <span>Display Address to other Alumni</span>
                    </div>
                </div>
            </div>

            <div class="form-actions">
                <button type="submit" class="btn btn-primary btn-lg" [disabled]="saving()">
                    {{ saving() ? 'Updating Profile...' : 'Save All Changes' }}
                </button>
            </div>
          </form>
        </div>
      }
    </div>
  `,
    styles: [`
    .profile-page { max-width: 900px; margin: 0 auto; display: flex; flex-direction: column; gap: 2rem; }
    .header { text-align: center; }
    
    .membership-status-card { 
        padding: 3rem; background: linear-gradient(135deg, var(--primary-color), var(--primary-light)); color: white; border: none;
    }
    .status-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2.5rem; }
    .m-number { font-family: monospace; letter-spacing: 2px; opacity: 0.8; font-size: 0.9rem; }
    .m-type { color: var(--accent-color); font-weight: 800; text-transform: uppercase; font-size: 0.75rem; letter-spacing: 1px; margin-top: 0.5rem; }
    .badge-icon { padding: 0.5rem 1rem; border-radius: 20px; font-weight: 800; font-size: 0.7rem; text-transform: uppercase; border: 1px solid rgba(255,255,255,0.2); }
    .badge-icon.active { background: var(--accent-color); color: #111; }
    .status-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 2rem; padding-top: 2rem; border-top: 1px solid rgba(255,255,255,0.2); }
    .grid-item small { opacity: 0.6; font-size: 0.7rem; text-transform: uppercase; font-weight: 700; }
    .grid-item p { font-weight: 700; margin-top: 0.25rem; font-size: 1.1rem; }

    .profile-card { padding: 3rem; }
    .section { margin-bottom: 3.5rem; }
    .section h4 { border-bottom: 2px solid var(--accent-color); padding-bottom: 0.75rem; margin-bottom: 2rem; color: var(--primary-color); }
    
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 2rem; }
    .full-width { grid-column: span 2; }
    .form-group label { display: block; font-weight: 700; font-size: 0.8rem; margin-bottom: 0.75rem; color: var(--text-muted); }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 0.9rem; border: 1px solid var(--border-color); border-radius: 8px; transition: 0.3s; background: var(--bg-color); color: var(--text-main); }
    .form-group input:focus, .form-group select:focus { border-color: var(--primary-color); outline: none; }

    .toggle-list { display: flex; flex-direction: column; gap: 1.25rem; margin-top: 2rem; }
    .toggle-item { display: flex; align-items: center; gap: 1.25rem; font-weight: 600; font-size: 0.95rem; }

    .form-actions { margin-top: 4rem; text-align: center; }

    .switch { position: relative; display: inline-block; width: 45px; height: 24px; }
    .switch input { opacity: 0; width: 0; height: 0; }
    .slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: #ccc; transition: .4s; border-radius: 24px; }
    .slider:before { position: absolute; content: ""; height: 18px; width: 18px; left: 3px; bottom: 3px; background-color: white; transition: .4s; border-radius: 50%; }
    input:checked + .slider { background-color: var(--primary-color); }
    input:checked + .slider:before { transform: translateX(21px); }

    .loading-box { padding: 4rem; text-align: center; }

    @media (max-width: 768px) {
        .form-grid { grid-template-columns: 1fr; }
        .full-width { grid-column: span 1; }
        .status-grid { grid-template-columns: 1fr; gap: 1.5rem; }
        .membership-status-card { padding: 2rem; }
    }
  `]
})
export class Profile implements OnInit {
    private profileService = inject(ProfileService);
    private notify = inject(NotificationService);

    loading = signal(true);
    saving = signal(false);
    profile: any = {};

    ngOnInit() {
        this.profileService.getProfile().subscribe({
            next: (p) => {
                this.profile = { ...p };
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getMembershipType(type: any): string {
        const types = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Life'];
        return types[type] || 'General';
    }

    getDegreeName(degree: any): string {
        const degrees = ['HSC', 'Bachelor', 'Masters', 'PhD', 'Other'];
        // Handle both string and numeric types if they exist
        if (typeof degree === 'number') return degrees[degree] || 'Degree';
        return degree;
    }

    updateProfile() {
        this.saving.set(true);
        this.profileService.updateProfile(this.profile).subscribe({
            next: () => {
                this.notify.success('Profile updated successfully!');
                this.saving.set(false);
            },
            error: () => {
                this.saving.set(false);
                this.notify.error('Update failed. Please check your connection and try again.');
            }
        });
    }
}
