import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ProfileService } from '../../core/services/profile.service';
import { MemberProfile } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, getCurrentECPosition, EC_ROLES, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getCategoryLabel, getMembershipTypeLabel, GENDER_OPTIONS, BLOOD_GROUP_OPTIONS, getBloodGroupName, TSHIRT_SIZES } from '../../core/constants/app.constants';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule],
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

    loading = signal(true);
    saving = signal(false);
    uploadingPhoto = signal(false);
    photoPreview = signal<string | null>(null);
    private photoFile: File | null = null;
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
            next: (p) => {
                this.profile = { ...p };
                if (!this.profile.academicHistory) this.profile.academicHistory = [];
                if (!this.profile.professionalHistory) this.profile.professionalHistory = [];
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
            startDate: new Date().toISOString().split('T')[0],
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
}
