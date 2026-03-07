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
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
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
}
