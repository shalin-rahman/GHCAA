import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, getAcademicYears, PROFESSIONAL_SECTORS } from '../../core/constants/app.constants';

@Component({
    selector: 'app-directory',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './directory.html',
    styleUrl: './directory.scss'
})
export class Directory implements OnInit {
    getECPositionName = getECPositionName;
    private networkService = inject(NetworkingService);
    private notify = inject(NotificationService);
    private router = inject(Router);

    members = signal<any[]>([]);
    loading = signal(true);
    years: number[] = getAcademicYears();
    sectors = PROFESSIONAL_SECTORS;
    selectedMember = signal<any | null>(null);

    filters = {
        query: '',
        year: null as number | null,
        sector: '',
        bloodGroup: ''
    };



    ngOnInit() {
        this.search();
    }

    search() {
        this.loading.set(true);
        this.networkService.searchMembers(this.filters).subscribe({
            next: (data) => {
                this.members.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    viewProfile(id: number) {
        const m = this.members().find(x => x.id === id);
        if (m) {
            this.selectedMember.set(m);
        }
    }

    sendMessage(id: number) {
        this.router.navigate(['/portal/messages'], { queryParams: { thread: id } });
        this.selectedMember.set(null);
    }
}
