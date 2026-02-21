import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RegistrationService } from '../../core/services/registration.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="registration-page">
      <div class="container overflow-hidden">
        <div class="wizard-container glass-card overflow-hidden">
          <!-- Wizard Sidebar / Progress -->
          <div class="wizard-sidebar hide-mobile">
            <div class="brand-box">
              <img src="/assets/logo.jpg" alt="GHCAA">
              <h3>Member Registry</h3>
            </div>
            <div class="steps-list">
              <div class="step-link" [class.active]="currentStep() === 1" [class.done]="currentStep() > 1">
                <span class="step-num">{{ currentStep() > 1 ? '✓' : '01' }}</span>
                <span class="step-txt">Personal</span>
              </div>
              <div class="step-link" [class.active]="currentStep() === 2" [class.done]="currentStep() > 2">
                <span class="step-num">{{ currentStep() > 2 ? '✓' : '02' }}</span>
                <span class="step-txt">Contact</span>
              </div>
              <div class="step-link" [class.active]="currentStep() === 3" [class.done]="currentStep() > 3">
                <span class="step-num">{{ currentStep() > 3 ? '✓' : '03' }}</span>
                <span class="step-txt">Academic</span>
              </div>
              <div class="step-link" [class.active]="currentStep() === 4" [class.done]="currentStep() > 4">
                <span class="step-num">{{ currentStep() > 4 ? '✓' : '04' }}</span>
                <span class="step-txt">Professional</span>
              </div>
              <div class="step-link" [class.active]="currentStep() === 5" [class.done]="submitted()">
                <span class="step-num">{{ submitted() ? '✓' : '05' }}</span>
                <span class="step-txt">Finalize</span>
              </div>
            </div>
            <div class="sidebar-help">
              <p>Guidance Required?</p>
              <a href="mailto:haragangian@gmail.com">haragangian&#64;gmail.com</a>
            </div>
          </div>

          <!-- Wizard Body -->
          <div class="wizard-body">
            <div class="mobile-progress show-mobile">
              <div class="p-bar" [style.width]="(currentStep() * 20) + '%'"></div>
              <span>Step {{ currentStep() }} of 5</span>
            </div>

            @if (!submitted()) {
              <form #regForm="ngForm" (submit)="onSubmit(regForm)" class="step-form">
                
                <!-- Step 1: Personal Details -->
                <div class="step-content reveal-text" *ngIf="currentStep() === 1">
                  <div class="step-header">
                    <h2>Identity & Profile</h2>
                    <p>Enter your primary identity details as per academic registers.</p>
                  </div>
                  <div class="form-grid">
                    <div class="form-group full-width">
                      <label>Full Legal Name *</label>
                      <input type="text" [(ngModel)]="model.FullName" name="fullName" required placeholder="Full Name as per SSC Certificate">
                    </div>
                    <div class="form-group">
                      <label>Father's Name *</label>
                      <input type="text" [(ngModel)]="model.FatherName" name="fatherName" required placeholder="Father's Name">
                    </div>
                    <div class="form-group">
                      <label>Mother's Name *</label>
                      <input type="text" [(ngModel)]="model.MotherName" name="motherName" required placeholder="Mother's Name">
                    </div>
                    <div class="form-group">
                      <label>Date of Birth *</label>
                      <input type="date" [(ngModel)]="model.DateOfBirth" name="dateOfBirth" required>
                    </div>
                    <div class="form-group">
                      <label>Gender *</label>
                      <select [(ngModel)]="model.Gender" name="gender" required>
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Other">Other</option>
                      </select>
                    </div>
                    <div class="form-group">
                      <label>Blood Group *</label>
                      <select [(ngModel)]="model.BloodGroup" name="bloodGroup" required>
                        <option value="APositive">A+</option><option value="ANegative">A-</option>
                        <option value="BPositive">B+</option><option value="BNegative">B-</option>
                        <option value="OPositive">O+</option><option value="ONegative">O-</option>
                        <option value="ABPositive">AB+</option><option value="ABNegative">AB-</option>
                      </select>
                    </div>
                    <div class="form-group">
                      <label>National ID (NID) *</label>
                      <input type="text" [(ngModel)]="model.NID" name="nid" required placeholder="NID Number">
                    </div>
                  </div>
                </div>

                <!-- Step 2: Contact Details -->
                <div class="step-content reveal-text" *ngIf="currentStep() === 2">
                  <div class="step-header">
                    <h2>Reachability</h2>
                    <p>Provide verified communication channels for official correspondence.</p>
                  </div>
                  <div class="form-grid">
                    <div class="form-group">
                      <label>Primary Email *</label>
                      <input type="email" [(ngModel)]="model.Email" name="email" required placeholder="alumni@example.com">
                    </div>
                    <div class="form-group">
                      <label>Verified Mobile *</label>
                      <input type="text" [(ngModel)]="model.MobileNo" name="mobileNo" required placeholder="01XXXXXXXXX">
                    </div>
                    <div class="form-group full-width">
                      <label>Present Address *</label>
                      <textarea [(ngModel)]="model.PresentAddress" name="presentAddress" required rows="2" placeholder="House, Street, Area, District"></textarea>
                    </div>
                    <div class="form-group full-width">
                      <label>Permanent Home Address *</label>
                      <textarea [(ngModel)]="model.PermanentAddress" name="permanentAddress" required rows="2" placeholder="Village/Area, PO, PS, District"></textarea>
                    </div>
                  </div>
                </div>

                <!-- Step 3: Academic History -->
                <div class="step-content reveal-text" *ngIf="currentStep() === 3">
                  <div class="step-header">
                    <h2>Academic Credentials</h2>
                    <p>Verify your degree years and subject groups at GHC.</p>
                  </div>
                  <div class="form-grid">
                    <div class="form-group full-width">
                      <label>Core Subject / Group *</label>
                      <input type="text" [(ngModel)]="model.SubjectGroup" name="subjectGroup" required placeholder="e.g. Science, Bangla, Commerce">
                    </div>
                    <div class="form-group">
                      <label>Highest Degree at GHC *</label>
                      <select [(ngModel)]="model.LastDegreeFromGHC" name="lastDegreeFromGHC" required>
                        <option value="HSC">HSC</option>
                        <option value="Bachelor">Bachelor</option>
                        <option value="Masters">Masters</option>
                        <option value="PhD">PhD</option>
                        <option value="Other">Other</option>
                      </select>
                    </div>
                    <div class="form-group">
                      <label>Certificate Passing Year *</label>
                      <input type="number" [(ngModel)]="model.GHCLastCertificatePassingYear" name="ghcLastCertificatePassingYear" required>
                    </div>
                    <div class="form-group">
                      <label>HSC Admission Year</label>
                      <input type="number" [(ngModel)]="model.HSCAdmissionYear" name="hscAdmissionYear" placeholder="YYYY">
                    </div>
                    <div class="form-group">
                      <label>GHC Admission Year</label>
                      <input type="number" [(ngModel)]="model.GHCAdmissionYear" name="ghcAdmissionYear" placeholder="YYYY">
                    </div>
                  </div>
                </div>

                <!-- Step 4: Professional & Emergency -->
                <div class="step-content reveal-text" *ngIf="currentStep() === 4">
                  <div class="step-header">
                    <h2>Vocation & Safety</h2>
                    <p>Current professional status and fallback contact point.</p>
                  </div>
                  <div class="form-grid">
                    <div class="form-grid inner-grid full-width">
                      <div class="form-group">
                        <label>Professional Sector *</label>
                        <input type="text" [(ngModel)]="model.ProfessionalSector" name="professionalSector" required placeholder="e.g. Govt. Service, IT">
                      </div>
                      <div class="form-group">
                        <label>Current Designation *</label>
                        <input type="text" [(ngModel)]="model.Designation" name="designation" required placeholder="e.g. Senior Officer">
                      </div>
                    </div>
                    <div class="form-group">
                      <label>Emergency Contact *</label>
                      <input type="text" [(ngModel)]="model.EmergencyContactName" name="emergencyContactName" required placeholder="Person Name">
                    </div>
                    <div class="form-group">
                      <label>Relation *</label>
                      <input type="text" [(ngModel)]="model.EmergencyContactRelation" name="emergencyContactRelation" required placeholder="e.g. Brother">
                    </div>
                    <div class="form-group full-width">
                      <label>Emergency Phone *</label>
                      <input type="text" [(ngModel)]="model.EmergencyContactPhone" name="emergencyContactPhone" required placeholder="Contact Number">
                    </div>
                  </div>
                </div>

                <!-- Step 5: Uploads & Agreement -->
                <div class="step-content reveal-text" *ngIf="currentStep() === 5">
                  <div class="step-header">
                    <h2>Registry Filing</h2>
                    <p>Attach necessary documentation to formalize your registry.</p>
                  </div>
                  <div class="form-grid">
                    <div class="form-group">
                      <label>Formal Profile Photo *</label>
                      <div class="file-drop" [class.has-file]="files['photo']">
                        <input type="file" (change)="onFileSelect($event, 'photo')" accept="image/*" required>
                        <p>{{ files['photo'] ? files['photo'].name : 'Drag or click to upload photo' }}</p>
                        @if (files['photo']) { <span class="file-check">✓</span> }
                      </div>
                    </div>
                    <div class="form-group">
                      <label>GHC Certificate Copy *</label>
                      <div class="file-drop" [class.has-file]="files['certificate']">
                        <input type="file" (change)="onFileSelect($event, 'certificate')" accept=".pdf,image/*" required>
                        <p>{{ files['certificate'] ? files['certificate'].name : 'Drag or click to upload certificate' }}</p>
                        @if (files['certificate']) { <span class="file-check">✓</span> }
                      </div>
                    </div>
                    <div class="form-group full-width">
                      <div class="consent-box glass-card">
                        <label class="checkbox-container">
                          <input type="checkbox" name="agree" required ngModel>
                          <span class="checkmark"></span>
                          I solemnly affirm that the data provided is accurate. I pledge to uphold the GHCAA Constitution and maintain professional decorum.
                        </label>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="wizard-actions">
                  <div class="left-actions">
                    <button type="button" class="btn btn-secondary" (click)="prevStep()" *ngIf="currentStep() > 1">Back</button>
                  </div>
                  <div class="right-actions">
                    <button type="button" class="btn btn-primary" (click)="nextStep()" *ngIf="currentStep() < 5">Continue</button>
                    <button type="submit" class="btn btn-accent" *ngIf="currentStep() === 5" [disabled]="loading() || regForm.invalid">
                      {{ loading() ? 'Processing...' : 'Finalize Registry' }}
                    </button>
                  </div>
                </div>
              </form>
            } @else {
              <div class="otp-stage reveal-text">
                <div class="icon-box">🏦</div>
                <h2>Security Verification</h2>
                <p>An authentication code has been transmitted to <strong>{{ model.email }}</strong>. Post completion, your status will be queued for audit.</p>
                <div class="otp-field">
                  <input type="text" [(ngModel)]="otpCode" placeholder="000000" maxlength="6">
                </div>
                <button (click)="verify()" class="btn btn-primary btn-lg w-full" [disabled]="loading()">
                   {{ loading() ? 'Authenticating...' : 'Confirm Identity' }}
                </button>
                <small class="text-muted">Problem receiving? <a href="#" (click)="$event.preventDefault()">Resend Security Code</a></small>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './register.scss'
})
export class Register {
  private regService = inject(RegistrationService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  loading = signal(false);
  submitted = signal(false);
  currentStep = signal(1);

  model: any = {
    FullName: '',
    FatherName: '',
    MotherName: '',
    DateOfBirth: '',
    Gender: 'Male',
    BloodGroup: 'APositive',
    NID: '',
    MobileNo: '',
    Email: '',
    PresentAddress: '',
    PermanentAddress: '',
    HSCAdmissionYear: 2024,
    GHCAdmissionYear: 2024,
    LastDegreeFromGHC: 'Bachelor',
    SubjectGroup: '',
    GHCLastCertificatePassingYear: 2024,
    ProfessionalSector: '',
    Designation: '',
    EmergencyContactName: '',
    EmergencyContactRelation: '',
    EmergencyContactPhone: ''
  };

  files: { [key: string]: File } = {};
  otpCode = '';

  nextStep() {
    if (this.currentStep() < 5) {
      this.currentStep.update(s => s + 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  onFileSelect(event: any, key: string) {
    const file = event.target.files[0];
    if (file) {
      this.files[key] = file;
    }
  }

  onSubmit(form: any) {
    if (form.invalid) return;
    this.loading.set(true);

    const formData = new FormData();
    Object.keys(this.model).forEach(key => {
      formData.append(key, this.model[key]);
    });

    if (this.files['photo']) formData.append('photo', this.files['photo']);
    if (this.files['certificate']) formData.append('certificate', this.files['certificate']);

    this.regService.register(formData).subscribe({
      next: () => {
        this.loading.set(false);
        this.submitted.set(true);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: (err) => {
        this.loading.set(false);
        this.notify.error(err.error?.message || 'Registration failed. Please try again.');
      }
    });
  }

  verify() {
    if (!this.otpCode) return;
    this.loading.set(true);
    this.regService.verifyEmail(this.model.Email, this.otpCode).subscribe({
      next: () => {
        this.loading.set(false);
        this.notify.success('Email verified! Your application is now pending review.');
        this.router.navigate(['/login']);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Incorrect or expired verification code. Please try again.');
      }
    });
  }
}
