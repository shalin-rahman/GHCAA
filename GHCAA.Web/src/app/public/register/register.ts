import { Component, inject, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RegistrationService } from '../../core/services/registration.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, BLOOD_GROUP_OPTIONS, GENDER_OPTIONS } from '../../core/constants/app.constants';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register implements OnDestroy {
  private regService = inject(RegistrationService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  loading = signal(false);
  submitted = signal(false);
  currentStep = signal(1);
  maxStepReached = signal(1);
  showTerms = signal(false);
  resendCooldown = signal(0);
  private timerInterval: any;
  ACADEMIC = ACADEMIC_DATA;
  IS_HSC = IS_HSC;
  years = this.ACADEMIC.getYears();
  certificateOptions = this.ACADEMIC.certificates;
  groupOptions = this.ACADEMIC.groups;
  subjectOptions = this.ACADEMIC.subjects;
  sectorOptions = this.ACADEMIC.sectors;
  bloodGroupOptions = BLOOD_GROUP_OPTIONS;
  genderOptions = GENDER_OPTIONS;
  paymentConfigs = signal<any[]>([]);

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
    EmergencyContactPhone: '',
    AcademicHistory: [
      { institutionName: 'Govt. Haraganga College', degree: 'HSC', subject: 'None', passingYear: null, isGHC: true }
    ],
    ProfessionalHistory: [],
    HasAcceptedTerms: false
  };

  addAcademic() {
    this.model.AcademicHistory.push({ institutionName: '', degree: '', subject: '', passingYear: null, isGHC: false });
  }

  removeAcademic(idx: number) {
    this.model.AcademicHistory.splice(idx, 1);
  }

  addProfessional() {
    this.model.ProfessionalHistory.push({ organizationName: '', designation: '', sector: '', location: '', startDate: '', isCurrent: false });
  }

  removeProfessional(idx: number) {
    this.model.ProfessionalHistory.splice(idx, 1);
  }

  files: { [key: string]: File } = {};
  otpCode = '';

  ngOnInit() {
    this.loadPaymentInfo();
  }

  loadPaymentInfo() {
    this.regService.getPublicPaymentConfigs().subscribe({
      next: (configs: any[]) => this.paymentConfigs.set(configs)
    });
  }

  nextStep() {
    if (this.currentStep() < 5) {
      this.currentStep.update(s => s + 1);
      if (this.currentStep() > this.maxStepReached()) {
        this.maxStepReached.set(this.currentStep());
      }
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  goToStep(step: number) {
    if (step <= this.maxStepReached()) {
      this.currentStep.set(step);
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
    if (form.invalid) {
      this.notify.error('Please complete all mandatory fields and provide necessary files.');
      return;
    }
    if (this.loading()) return;
    this.loading.set(true);

    // Clean data before submission
    if (this.model.NID) this.model.NID = this.model.NID.replace(/\s/g, '');
    if (this.model.Email) this.model.Email = this.model.Email.trim().toLowerCase();

    const formData = new FormData();
    Object.keys(this.model).forEach(key => {
      if (key !== 'AcademicHistory' && key !== 'ProfessionalHistory') {
        formData.append(key, this.model[key]);
      }
    });

    // Append Academic History as array
    this.model.AcademicHistory.forEach((item: any, i: number) => {
      formData.append(`AcademicHistory[${i}].InstitutionName`, item.institutionName);
      formData.append(`AcademicHistory[${i}].Degree`, item.degree);
      formData.append(`AcademicHistory[${i}].Subject`, item.subject);
      formData.append(`AcademicHistory[${i}].AdmissionYear`, item.admissionYear || '');
      formData.append(`AcademicHistory[${i}].PassingYear`, item.passingYear || '');
      formData.append(`AcademicHistory[${i}].IsGHC`, item.isGHC.toString());
    });

    // Append Professional History as array
    this.model.ProfessionalHistory.forEach((item: any, i: number) => {
      formData.append(`ProfessionalHistory[${i}].OrganizationName`, item.organizationName);
      formData.append(`ProfessionalHistory[${i}].Designation`, item.designation);
      formData.append(`ProfessionalHistory[${i}].Sector`, item.sector || '');
      formData.append(`ProfessionalHistory[${i}].Location`, item.location || '');
      formData.append(`ProfessionalHistory[${i}].StartDate`, item.startDate || '');
      if (item.endDate) formData.append(`ProfessionalHistory[${i}].EndDate`, item.endDate);
      formData.append(`ProfessionalHistory[${i}].IsCurrent`, item.isCurrent.toString());
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

  resendOtp() {
    if (this.resendCooldown() > 0 || this.loading()) return;
    
    this.loading.set(true);
    this.regService.resendOtp(this.model.Email).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        this.notify.success(res.message || 'Verification code resent successfully!');
        this.startResendTimer();
      },
      error: (err: any) => {
        this.loading.set(false);
        this.notify.error(err.error?.message || 'Failed to resend code. Please try again.');
        this.startResendTimer(); // Start timer anyway to prevent spam
      }
    });
  }

  private startResendTimer() {
    this.resendCooldown.set(60);
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.timerInterval = setInterval(() => {
      const current = this.resendCooldown();
      if (current > 0) {
        this.resendCooldown.set(current - 1);
      } else {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }
}


