import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ProfileService } from '../../core/services/profile.service';
import { MemberProfile } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, getCurrentECPosition, EC_ROLES, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getCategoryLabel, getMembershipTypeLabel, GENDER_OPTIONS, BLOOD_GROUP_OPTIONS, getBloodGroupName, TSHIRT_SIZES } from '../../core/constants/app.constants';
import { DatePipe } from '@angular/common';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent],
    providers: [DatePipe],
    templateUrl: './profile.html',
    styleUrl: './profile.scss'
})
export class Profile implements OnInit {
    getECPositionName = getECPositionName;
    getCurrentECPosition = getCurrentECPosition;
    getBloodGroupName = getBloodGroupName;
    ecRoles = EC_ROLES;
    private profileService = inject(ProfileService);
    private notify = inject(NotificationService);
    private datePipe = inject(DatePipe);

    loading = signal(true);
    saving = signal(false);
    uploadingPhoto = signal(false);
    uploadingSignature = signal(false);
    photoPreview = signal<string | null>(null);
    signaturePreview = signal<string | null>(null);
    private photoFile: File | null = null;
    private signatureFile: File | null = null;
    profile: any = {};
    ACADEMIC = ACADEMIC_DATA;
    yearsList = this.ACADEMIC.getYears();
    IS_HSC = IS_HSC;
    degreeOptions = this.ACADEMIC.certificates;
    subjectOptions = this.ACADEMIC.subjects;
    sectorOptions = this.ACADEMIC.sectors;
    genderOptions = GENDER_OPTIONS;
    bloodGroupOptions = BLOOD_GROUP_OPTIONS;
    tShirtOptions = TSHIRT_SIZES;

    ngOnInit() {
        this.profileService.getProfile().subscribe({
            next: (p: any) => {
                // Robust case-insensitive property mapping
                const mapping = (obj: any) => {
                    const result: any = {};
                    const props = [
                        'id', 'fullName', 'mobileNo', 'email', 'fatherName', 'motherName', 'dateOfBirth', 
                        'nid', 'gender', 'bloodGroup', 'presentAddress', 'permanentAddress', 
                        'emergencyContactName', 'emergencyContactRelation', 'emergencyContactPhone',
                        'membershipNumber', 'membershipType', 'category', 'status', 'photoPath', 'signaturePath',
                        'isVerified', 'contributionPoints', 'tShirtSize', 'isMobilePublic', 'isEmailPublic',
                        'isAddressPublic', 'isNIDPublic', 'isFamilyPublic', 'notifyEventCreation',
                        'notifyParticipationApproval', 'notifyRegistrationUpdate', 'notifyRelevantUpdates',
                        'certificatePath', 'paymentProofPath'
                    ];
                    props.forEach(prop => {
                        const pascal = prop.charAt(0).toUpperCase() + prop.slice(1);
                        result[prop] = obj[prop] !== undefined ? obj[prop] : (obj[pascal] !== undefined ? obj[pascal] : (prop === 'nid' ? obj['NID'] : undefined));
                    });
                    result.academicHistory = obj.academicHistory || obj.AcademicHistory || [];
                    result.professionalHistory = obj.professionalHistory || obj.ProfessionalHistory || [];
                    return result;
                };

                this.profile = mapping(p);
                
                if (this.profile.dateOfBirth) {
                    this.profile.dateOfBirth = this.datePipe.transform(this.profile.dateOfBirth, 'dd-MM-yyyy') || '';
                }
                
                if (this.profile.professionalHistory) {
                    this.profile.professionalHistory = this.profile.professionalHistory.map((ph: any) => ({
                        ...ph,
                        startDate: this.datePipe.transform(ph.startDate || ph.StartDate, 'dd-MM-yyyy') || '',
                        endDate: ph.endDate || ph.EndDate ? this.datePipe.transform(ph.endDate || ph.EndDate, 'dd-MM-yyyy') : ''
                    }));
                }
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    addAcademicRecord() {
        this.profile.academicHistory.push({
            institutionName: '',
            degree: 'HSC',
            subject: 'None',
            passingYear: new Date().getFullYear(),
            isGHC: false
        });
    }

    removeAcademicRecord(index: number) {
        if (!confirm('Remove this academic record?')) return;
        this.profile.academicHistory.splice(index, 1);
    }

    addProfessionalRecord() {
        if (!this.profile.professionalHistory) this.profile.professionalHistory = [];
        this.profile.professionalHistory.push({
            organizationName: '',
            designation: '',
            sector: 'Other',
            location: '',
            startDate: this.datePipe.transform(new Date(), 'dd-MM-yyyy') || '',
            isCurrent: true
        });
    }


    removeProfessionalRecord(index: number) {
        if (!confirm('Remove this professional record?')) return;
        this.profile.professionalHistory.splice(index, 1);
    }

    removePhoto() {
        if (!confirm('Remove your profile photo?')) return;
        this.profile.photoPath = null;
        this.photoFile = null;
        this.photoPreview.set(null);
    }

    getMembershipType(type: any): string {
        return getMembershipTypeLabel(type);
    }

    getCategoryLabel(cat: any): string {
        return getCategoryLabel(cat);
    }

    getAvailableSubjects(degree: string): string[] {
        return this.subjectOptions;
    }

    getMajorDisplay(degree: string, subject: string): string {
        return subject && subject !== 'None' ? `in ${subject}` : '';
    }

    getImageUrl(path: string | null | undefined): string {
        if (!path) return '';
        if (path.startsWith('http')) return path;
        const cleanPath = path.startsWith('/') ? path : '/' + path;
        return cleanPath.replace(/^\/\//, '/');
    }

    getGHCHistory() {
        if (!this.profile.academicHistory) return null;
        const ghc = this.profile.academicHistory.filter((a: any) => a.isGHC);
        if (ghc.length === 0) return null;
        return ghc.sort((a: any, b: any) => (b.passingYear || 0) - (a.passingYear || 0))[0];
    }

    getHighestHistory() {
        if (!this.profile.academicHistory || this.profile.academicHistory.length === 0) return null;
        return this.profile.academicHistory.sort((a: any, b: any) => (b.passingYear || 0) - (a.passingYear || 0))[0];
    }

    async updateProfile(form: any) {
        if (form.invalid) {
            form.control.markAllAsTouched();
            this.notify.error('Please correct all validation errors in the form before saving.');
            return;
        }

        if (this.saving()) return;
        ensureValidAcademicData(this.profile);

        this.saving.set(true);
        try {
            // 1. Sync photo if pending
            if (this.photoFile) {
                const res = await firstValueFrom(this.profileService.uploadPhoto(this.photoFile));
                this.profile.photoPath = res.photoPath;
                this.photoFile = null;
                this.photoPreview.set(null);
            }

            // 1b. Sync signature if pending
            if (this.signatureFile) {
                const res = await firstValueFrom(this.profileService.uploadSignature(this.signatureFile));
                this.profile.signaturePath = res.signaturePath;
                this.signatureFile = null;
                this.signaturePreview.set(null);
            }

            // 2. Sync Metadata
            await firstValueFrom(this.profileService.updateProfile(this.profile));
            
            this.notify.success('Profile information updated');
            
            // 3. Force re-sync from server to ensure UI is exact
            this.profileService.getProfile().subscribe(p => {
                this.profile = { ...p };
                if (!this.profile.academicHistory) this.profile.academicHistory = [];
                if (!this.profile.professionalHistory) this.profile.professionalHistory = [];
            });

        } catch (error: any) {
            this.notify.error(error?.error?.message || 'Update failed. Please check your network.');
        } finally {
            this.saving.set(false);
        }
    }

    onPhotoSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;
        this.photoFile = file;
        const reader = new FileReader();
        reader.onload = (e) => this.photoPreview.set(e.target?.result as string);
        reader.readAsDataURL(file);
    }

    uploadPhoto() {
        if (!this.photoFile) return;
        this.uploadingPhoto.set(true);
        this.profileService.uploadPhoto(this.photoFile).subscribe({
            next: (res) => {
                this.profile.photoPath = res.photoPath;
                this.photoFile = null;
                this.photoPreview.set(null);
                this.uploadingPhoto.set(false);
                this.notify.success('Profile photo updated successfully!');
            },
            error: (err) => {
                this.uploadingPhoto.set(false);
                this.notify.error(err?.error?.message || 'Photo upload failed.');
            }
        });
    }

    onSignatureSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;
        this.signatureFile = file;
        const reader = new FileReader();
        reader.onload = (e) => this.signaturePreview.set(e.target?.result as string);
        reader.readAsDataURL(file);
    }

    removeSignature() {
        if (!confirm('Remove your signature?')) return;
        this.profile.signaturePath = null;
        this.signatureFile = null;
        this.signaturePreview.set(null);
    }
}
