import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ProfileService } from '../../core/services/profile.service';
import { MemberProfile } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { getECPositionName, getCurrentECPosition, EC_ROLES, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getCategoryLabel, getMembershipTypeLabel, getBloodGroupName, TSHIRT_SIZES, LOOKUP_GROUPS } from '../../core/constants/app.constants';
import { LookupService, LookupOption } from '../../core/services/lookup.service';
import { DatePipe } from '@angular/common';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { toWireDate } from '../../core/utils/date.util';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { Icon } from '../../common/icon/icon';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, ImgFallbackDirective, Icon],
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
    private confirmDialog = inject(ConfirmDialogService);
    private datePipe = inject(DatePipe);
    private lookupService = inject(LookupService);
    readonly orgConfig = inject(OrgConfigService);

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
    IS_HSC = IS_HSC;
    degreeOptions = this.ACADEMIC.certificates;
    subjectOptions = this.ACADEMIC.subjects;
    sectorOptions = this.ACADEMIC.sectors;

    // 82.42: sourced from /lookups/{group} via LookupService, filled in ngOnInit.
    yearsList: number[] = [];
    genderOptions: LookupOption[] = [];
    bloodGroupOptions: LookupOption[] = [];
    tShirtOptions = TSHIRT_SIZES;

    ngOnInit() {
        this.profileService.getProfile().subscribe({
            next: (p: any) => {
                this.applyProfileResponse(p);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
        this.lookupService.getAcademicYears().subscribe(years => this.yearsList = years);
        this.lookupService.getOptions(LOOKUP_GROUPS.Gender).subscribe(opts => this.genderOptions = opts);
        this.lookupService.getOptions(LOOKUP_GROUPS.BloodGroup).subscribe(opts => this.bloodGroupOptions = opts);
    }

    // 29D.8: Single normalization path for a profile response (case-insensitive property
    // mapping + display-date formatting). Previously only ngOnInit applied this; the
    // post-save re-sync did `this.profile = {...p}` on the RAW response, so PascalCase keys
    // and ISO dates leaked through and half the form fields rendered blank after saving.
    private applyProfileResponse(p: any) {
        // 32.3: was a hardcoded ~30-field whitelist, which silently dropped any DTO field not
        // explicitly listed (designation, professionalSector, profileCompletionPercentage, etc.)
        // — replaced with a generic case-insensitive key normalization so every current and
        // future flat DTO field survives.
        const mapping = (obj: any) => {
            const result: any = {};
            Object.keys(obj || {}).forEach(key => {
                const camelKey = key === key.toUpperCase()
                    ? key.toLowerCase()
                    : key.charAt(0).toLowerCase() + key.slice(1);
                if (result[camelKey] === undefined) {
                    result[camelKey] = obj[key];
                }
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
    }

    addAcademicRecord() {
        this.profile.academicHistory.push({
            institutionName: '',
            degree: 'HSC',
            subject: 'None',
            passingYear: new Date().getFullYear(),
            isOrgProfile: false
        });
    }

    async removeAcademicRecord(index: number) {
        if (this.isInstitutionalAcademicRecord(index)) return;
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove academic record',
            message: 'Remove this academic record?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!ok) return;
        this.profile.academicHistory.splice(index, 1);
    }

    isInstitutionalAcademicRecord(index: number): boolean {
        const record = this.profile.academicHistory?.[index];
        const institutionName = this.orgConfig.config()?.branding?.institutionName;
        return index === 0 &&
            !!record &&
            !!institutionName &&
            record.institutionName?.trim().toLocaleLowerCase() ===
                institutionName.trim().toLocaleLowerCase();
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


    async removeProfessionalRecord(index: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove professional record',
            message: 'Remove this professional record?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!ok) return;
        this.profile.professionalHistory.splice(index, 1);
    }

    async removePhoto() {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove profile photo',
            message: 'Remove your profile photo?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!ok) return;
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
        const ghc = this.profile.academicHistory.filter((a: any) => a.isOrgProfile);
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

            // 2. Sync Metadata (convert display dd-MM-yyyy dates to ISO wire format on a copy)
            const payload = {
                ...this.profile,
                dateOfBirth: toWireDate(this.profile.dateOfBirth),
                professionalHistory: (this.profile.professionalHistory || []).map((ph: any) => ({
                    ...ph,
                    startDate: toWireDate(ph.startDate),
                    endDate: toWireDate(ph.endDate)
                }))
            };
            await firstValueFrom(this.profileService.updateProfile(payload));
            
            this.notify.success('Profile information updated');
            
            // 3. Force re-sync from server to ensure UI is exact — through the SAME
            // normalization path as the initial load so no fields are dropped (29D.8).
            this.profileService.getProfile().subscribe({
                next: p => this.applyProfileResponse(p),
                error: () => this.notify.error('Saved, but failed to refresh. Reload to see the latest.')
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

    async removeSignature() {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove signature',
            message: 'Remove your signature?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!ok) return;
        this.profile.signaturePath = null;
        this.signatureFile = null;
        this.signaturePreview.set(null);
    }
}
