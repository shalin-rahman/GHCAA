import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MentorshipService } from '../../core/services/mentorship.service';
import { MentorshipAdminRow } from '../../core/models/business.models';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { getMentorshipStatusLabel, getMentorshipStatusClass } from '../../core/constants/app.constants';

@Component({
    selector: 'app-admin-mentorship',
    standalone: true,
    imports: [CommonModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent],
    templateUrl: './admin-mentorship.html',
    styleUrl: './admin-mentorship.scss'
})
export class AdminMentorship implements OnInit {
    private mentorshipService = inject(MentorshipService);
    getMentorshipStatusLabel = getMentorshipStatusLabel;
    getMentorshipStatusClass = getMentorshipStatusClass;

    loading = signal(true);
    requests = signal<MentorshipAdminRow[]>([]);
    searchTerm = signal('');

    filteredRequests = computed(() => {
        const term = this.searchTerm().toLowerCase();
        if (!term) return this.requests();
        return this.requests().filter(r =>
            r.requester?.fullName?.toLowerCase().includes(term) ||
            r.mentor?.fullName?.toLowerCase().includes(term) ||
            r.domain?.toLowerCase().includes(term)
        );
    });

    ngOnInit() {
        this.loadRequests();
    }

    private loadRequests() {
        this.loading.set(true);
        this.mentorshipService.getAllForAdmin().subscribe({
            next: (data) => { this.requests.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }
}
