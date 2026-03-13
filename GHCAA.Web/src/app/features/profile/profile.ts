import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService, MemberProfile } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, EC_ROLES, ACADEMIC_DATA, IS_HSC, ensureValidAcademicData, getCategoryLabel, getMembershipTypeLabel, GENDER_OPTIONS, BLOOD_GROUP_OPTIONS } from '../../core/constants/app.constants';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './profile.html',
    styleUrl: './profile.scss'
})
export class Profile implements OnInit {
    getECPositionName = getECPositionName;
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
    groupOptions = this.ACADEMIC.groups;
    subjectOptions = this.ACADEMIC.subjects;
    sectorOptions = this.ACADEMIC.sectors;
    genderOptions = GENDER_OPTIONS;
    bloodGroupOptions = BLOOD_GROUP_OPTIONS;

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
        this.profile.academicHistory.splice(index, 1);
    }

    addProfessionalRecord() {
        this.profile.professionalHistory.push({
            organizationName: '',
            designation: '',
            startDate: new Date().toISOString().split('T')[0],
            isCurrent: false
        });
    }

    removeProfessionalRecord(index: number) {
        this.profile.professionalHistory.splice(index, 1);
    }

    getMembershipType(type: any): string {
        return getMembershipTypeLabel(type);
    }

    getCategoryLabel(cat: any): string {
        return getCategoryLabel(cat);
    }

    getDegreeName(degree: any): string {
        return degree;
    }

    updateProfile() {
        ensureValidAcademicData(this.profile);

        this.saving.set(true);
        this.profileService.updateProfile(this.profile).subscribe({
            next: () => {
                this.notify.success('Information updated');
                this.saving.set(false);
            },
            error: () => {
                this.saving.set(false);
                this.notify.error('Update failed. Please check your connection and try again.');
            }
        });
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
