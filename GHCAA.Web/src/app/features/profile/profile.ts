import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService, MemberProfile } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './profile.html',
    styleUrl: './profile.scss'
})
export class Profile implements OnInit {
    private profileService = inject(ProfileService);
    private notify = inject(NotificationService);

    loading = signal(true);
    saving = signal(false);
    profile: any = {};
    yearsList = Array.from({ length: 100 }, (_, i) => new Date().getFullYear() - i);
    sectorOptions = ['Govt. Service', 'Corporate', 'Business', 'Education', 'Medical/Health', 'Engineering', 'Law', 'Other'];

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
        const types = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory'];
        return types[type] || 'General';
    }

    getCategoryLabel(cat: any): string {
        const cats = ['None', 'Lifelong', 'Donor', 'Patron'];
        if (typeof cat === 'number') return cats[cat] || 'None';
        return cat || 'None';
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
