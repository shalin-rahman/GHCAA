import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RegistrationService } from '../../core/services/registration.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { ACADEMIC_DATA, IS_HSC, ensureValidAcademicData } from '../../core/constants/app.constants';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private regService = inject(RegistrationService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  loading = signal(false);
  submitted = signal(false);
  currentStep = signal(1);
  ACADEMIC = ACADEMIC_DATA;
  IS_HSC = IS_HSC;
  years = this.ACADEMIC.getYears();
  certificateOptions = this.ACADEMIC.certificates;
  groupOptions = this.ACADEMIC.groups;
  subjectOptions = this.ACADEMIC.subjects;
  sectorOptions = this.ACADEMIC.sectors;

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
    HSCAdmissionYear: null,
    HighestCertificate: 'HSC',
    HighestCertificateGroup: 'Science',
    HighestCertificateSubject: 'None',
    HighestCertificatePassingYear: null,
    GHCAdmissionYear: null,
    GHCLastCertificate: 'HSC',
    GHCLastCertificateGroup: 'Science',
    GHCLastCertificateSubject: 'None',
    GHCLastCertificatePassingYear: null,
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
    if (this.files['paymentProof']) formData.append('paymentProof', this.files['paymentProof']);

    // Auto-set for HSC logic
    ensureValidAcademicData(this.model);

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
