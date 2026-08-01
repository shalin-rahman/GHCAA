import { Component, inject, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RegistrationService } from '../../core/services/registration.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { PaymentPortalComponent } from '../../common/payment-portal/payment-portal.component';
import { FinancialService } from '../../core/services/financial.service';
import { ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, BLOOD_GROUP_OPTIONS, GENDER_OPTIONS, TSHIRT_SIZES, MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';
import { validateUploadFile } from '../../core/utils/file-validation.util';
import { parseDisplayDate } from '../../core/utils/date.util';
import { GatewaysService } from '../../core/services/gateways.service';
import { PaymentGateway } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { Icon } from '../../common/icon/icon';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, PaymentPortalComponent, LogoSpinnerComponent, Icon, ImgFallbackDirective],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register implements OnDestroy {
  private regService = inject(RegistrationService);
  private router = inject(Router);
  private notify = inject(NotificationService);
  private gatewaysService = inject(GatewaysService);
  private finService = inject(FinancialService);

  loading = signal(false);
  registrationFee = signal<number>(500); // Default placeholder
  submitted = signal(false);
  currentStep = signal(1);
  maxStepReached = signal(1);
  showTerms = signal(false);
  resendCooldown = signal(0);
  maxBirthDate = new Date(new Date().setFullYear(new Date().getFullYear() - 15)).toISOString().split('T')[0];
  paymentConfigs = signal<any[]>([]);
  selectedPaymentMethod = signal<any>(null);
  registrationResult = signal<any>(null);
  private timerInterval: any;
  ACADEMIC = ACADEMIC_DATA;
  IS_HSC = IS_HSC;
  years = this.ACADEMIC.getYears();
  certificateOptions = this.ACADEMIC.certificates;
  subjectOptions = this.ACADEMIC.subjects;
  sectorOptions = this.ACADEMIC.sectors;
  bloodGroupOptions = BLOOD_GROUP_OPTIONS;
  genderOptions = GENDER_OPTIONS;
  tShirtOptions = TSHIRT_SIZES;
  membershipTypeOptions = MEMBERSHIP_TYPE_OPTIONS;


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
    TShirtSize: 'L',
    MembershipType: 'General',
    EmergencyContactName: '',
    EmergencyContactRelation: '',
    EmergencyContactPhone: '',
    AcademicHistory: [
      { institutionName: 'Govt. Haraganga College', degree: 'HSC', subject: 'None', admissionYear: null, passingYear: null, isGHC: true }
    ],
    ProfessionalHistory: [
      { organizationName: '', designation: '', sector: '', location: '', startDate: '', isCurrent: false }
    ],
    HasAcceptedTerms: false,
    HasAcceptedGdpr: false,
    PaymentMethodId: 0,
    TransactionId: ''
  };

  getFilteredSubjects(degree: string) {
    const hscClusters = ['Science', 'Arts & Humanities', 'Business Studies'];
    if (degree === 'HSC') {
      return hscClusters;
    }
    // Return all subjects except the three general clusters
    return this.subjectOptions.filter(s => !hscClusters.includes(s));
  }

  addAcademic() {
    this.model.AcademicHistory.push({ institutionName: '', degree: '', subject: '', admissionYear: null, passingYear: null, isGHC: false });
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
    this.loadRegistrationFee();
  }

  loadRegistrationFee() {
    const type = this.model.MembershipType || 'General';
    this.finService.getApplicableFee('RegistrationFee', type).subscribe({
      next: (res) => this.registrationFee.set(res.amount),
      error: () => this.registrationFee.set(500) // fallback
    });
  }

  loadPaymentInfo() {
    this.regService.getPublicPaymentConfigs().subscribe({
      // 29F.2: surface HTTP failures instead of failing silently
      next: (configs: any[]) => this.paymentConfigs.set(configs),
      error: () => this.notify.error('Failed to load payment information.')
    });
  }

  onPaymentMethodChange(method: any) {
    this.selectedPaymentMethod.set(method);
    this.model.PaymentMethodId = method.id;
  }

  onReferenceSelected(val: string) {
    this.model.TransactionId = val;
  }

  onPaymentReceiptSelected(file: File) {
    this.files['paymentProof'] = file;
  }

  getValidYears() {
    if (!this.model.DateOfBirth) return this.years;
    
    let birthYear = NaN;
    if (typeof this.model.DateOfBirth === 'string' && this.model.DateOfBirth.includes('-')) {
      const parts = this.model.DateOfBirth.split('-');
      // Handle both YYYY-MM-DD and DD-MM-YYYY
      birthYear = parts[0].length === 4 ? parseInt(parts[0]) : parseInt(parts[2]);
    } else {
      birthYear = parseDisplayDate(this.model.DateOfBirth)?.getFullYear() ?? NaN;
    }

    if (isNaN(birthYear)) return this.years;
    const minYear = birthYear + 13; // Minimum age for SSC/HSC usually ~15-16, 13 is safe
    return this.years.filter(y => y >= minYear);
  }

  nextStep(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      this.notify.error('Please complete all mandatory fields correctly before proceeding.');
      return;
    }

    if (this.currentStep() < 3) {
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
    const file: File = event.target.files[0];
    if (!file) return;
    const kind = key === 'paymentProof' ? 'pdf' : 'image';
    const err = validateUploadFile(file, kind);
    if (err) { this.notify.error(err); event.target.value = ''; return; }
    this.files[key] = file;
  }

  onSubmit(form: any) {
    if (form.invalid || !this.model.PaymentMethodId || !this.files['photo']) {
      form.control.markAllAsTouched();
      this.notify.error('Please complete all mandatory fields, select a payment method and provide necessary files.');
      return;
    }

    // Birth year validation for academic records
    if (this.model.DateOfBirth) {
      const birthYear = parseDisplayDate(this.model.DateOfBirth)?.getFullYear() ?? NaN;
      for (const item of this.model.AcademicHistory) {
        if (item.admissionYear < birthYear + 15 || item.passingYear < birthYear + 15) {
          this.notify.error(`Academic milestones must be at least 15 years after your birth year (${birthYear}).`);
          return;
        }
      }
    }

    if (this.loading()) return;
    this.loading.set(true);

    // Clean data before submission
    if (this.model.NID) this.model.NID = this.model.NID.replace(/\s/g, '');
    if (this.model.Email) this.model.Email = this.model.Email.trim().toLowerCase();

    const formData = new FormData();
    Object.keys(this.model).forEach(key => {
      if (key !== 'AcademicHistory' && key !== 'ProfessionalHistory') {
        let val = this.model[key];
        if (key === 'DateOfBirth') val = this.formatDateForApi(val);
        formData.append(key, val);
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
      formData.append(`ProfessionalHistory[${i}].StartDate`, this.formatDateForApi(item.startDate) || '');
      if (item.endDate) formData.append(`ProfessionalHistory[${i}].EndDate`, this.formatDateForApi(item.endDate));
      formData.append(`ProfessionalHistory[${i}].IsCurrent`, item.isCurrent.toString());
    });

    if (this.files['photo']) formData.append('photo', this.files['photo']);
    if (this.files['certificate']) formData.append('certificate', this.files['certificate']);
    if (this.files['paymentProof']) formData.append('paymentProof', this.files['paymentProof']);

    // Auto-set for HSC logic
    ensureValidAcademicData(this.model);

    this.regService.register(formData).subscribe({
      next: (res) => {
        this.registrationResult.set(res);
        
        // Handle Online Payment Redirection
        if (this.selectedPaymentMethod()?.isOnline) {
          this.initiateGateway(res.memberId);
        } else {
          this.currentStep.set(4);
          this.notify.success('Registry filing submitted successfully. Please verify your email.');
        }
      },
      error: (err) => {
        this.notify.error(err.error?.message || 'Registration failed. Please check your data.');
        this.loading.set(false);
      }
    });
  }

  private formatDateForApi(dateStr: string): string {
    if (!dateStr || typeof dateStr !== 'string') return dateStr;
    const parts = dateStr.split('-');
    if (parts.length === 3 && parts[0].length === 2) {
      // dd-mm-yyyy to yyyy-mm-dd
      return `${parts[2]}-${parts[1]}-${parts[0]}`;
    }
    return dateStr;
  }

  private initiateGateway(memberId: number) {
    const method = this.selectedPaymentMethod();
    if (!method) return;

    const gatewayStr = method.gateway;
    const gateway = (gatewayStr && PaymentGateway[gatewayStr as keyof typeof PaymentGateway] !== undefined) 
      ? PaymentGateway[gatewayStr as keyof typeof PaymentGateway] 
      : PaymentGateway.None;

    this.gatewaysService.initiatePayment({
      amount: this.registrationFee(), // Dynamically fetched fee
      gateway: gateway,
      reference: this.model.TransactionId || `REG-${memberId}`,
      baseUrl: window.location.origin,
      customerName: this.model.FullName,
      customerEmail: this.model.Email,
      customerPhone: this.model.MobileNo
    }).subscribe({
      next: (res) => {
        if (res.success && res.gatewayUrl) {
          window.location.href = res.gatewayUrl;
        } else {
          this.notify.warning('Registry filed, but online payment initiation failed. Please verify with manual receipt or check dashboard.');
          this.currentStep.set(4);
        }
      },
      error: () => {
        this.notify.warning('Online payment initiation failed. Manual verification will be required.');
        this.currentStep.set(4);
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


