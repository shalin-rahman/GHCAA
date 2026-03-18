import { Component, inject, signal, OnInit, OnDestroy, Input, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, getCurrentECPosition, getAcademicYears, PROFESSIONAL_SECTORS, getBloodGroupName } from '../../core/constants/app.constants';

@Component({
    selector: 'app-directory',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './directory.html',
    styleUrl: './directory.scss'
})
export class Directory implements OnInit, AfterViewInit, OnDestroy {
    @Input() isCompact: boolean = false;
    @Input() disableProfile: boolean = false;
    @ViewChild('sentinel') sentinelRef!: ElementRef<HTMLElement>;

    getECPositionName = getECPositionName;
    getCurrentECPosition = getCurrentECPosition;
    getBloodGroupName = getBloodGroupName;
    private networkService = inject(NetworkingService);
    private notify = inject(NotificationService);
    private router = inject(Router);

    members = signal<MemberSummary[]>([]);
    loading = signal(true);      // initial/search load
    loadingMore = signal(false); // scroll-triggered load
    hasMore = signal(true);
    totalItems = signal(0);

    years: number[] = getAcademicYears();
    sectors = PROFESSIONAL_SECTORS;
    selectedMember = signal<any | null>(null);

    getMajorDisplay(degree: string | undefined, group: string | undefined, subject: string | undefined): string {
        if (!degree) return '';
        const major = degree === 'HSC' ? (group || 'None') : (subject || 'None');
        return major && major !== 'None' ? `in ${major}` : '';
    }

    private currentPage = 1;
    private readonly PAGE_SIZE = 20;
    private searchDebounce: any;
    private observer!: IntersectionObserver;

    @ViewChild('sentinel') set sentinel(element: ElementRef<HTMLElement>) {
        if (element) {
            this.setupIntersectionObserver(element.nativeElement);
        }
    }

    filters = {
        query: '',
        year: null as number | null,
        sector: '',
        bloodGroup: ''
    };

    ngOnInit() {
        this.doSearch(); // Initial load
    }

    ngAfterViewInit() {
        // Observer setup handled by ViewChild setter
    }

    ngOnDestroy() {
        if (this.observer) this.observer.disconnect();
        if (this.searchDebounce) clearTimeout(this.searchDebounce);
    }

    private setupIntersectionObserver(element: HTMLElement) {
        if (this.observer) this.observer.disconnect();
        
        this.observer = new IntersectionObserver(
            (entries) => {
                const entry = entries[0];
                if (entry.isIntersecting && this.hasMore() && !this.loading() && !this.loadingMore()) {
                    this.loadNextPage();
                }
            },
            { 
                root: null, // use viewport
                rootMargin: '600px', // Trigger even earlier
                threshold: 0.1
            }
        );

        this.observer.observe(element);
    }

    /** Called when filters change — resets to page 1 */
    search() {
        if (this.searchDebounce) clearTimeout(this.searchDebounce);
        this.searchDebounce = setTimeout(() => this.doSearch(), 300);
    }

    private doSearch() {
        this.currentPage = 1;
        this.members.set([]);
        this.hasMore.set(true);
        this.loading.set(true);
        this.fetchPage(1).then(() => {
            this.loading.set(false);
        });
    }

    loadNextPage() {
        if (!this.hasMore() || this.loadingMore()) return;
        this.loadingMore.set(true);
        const nextPage = this.currentPage + 1;
        this.fetchPage(nextPage).then(() => {
            this.loadingMore.set(false);
        });
    }

    private fetchPage(page: number): Promise<boolean> {
        return new Promise(resolve => {
            const apiFilter: any = {
                query: this.filters.query || undefined,
                passingYear: this.filters.year || undefined,
                professionalSector: this.filters.sector || undefined,
                bloodGroup: this.filters.bloodGroup || undefined,
                page,
                pageSize: this.PAGE_SIZE
            };

            // Remove undefined values
            Object.keys(apiFilter).forEach(k => { if (apiFilter[k] === undefined) delete apiFilter[k]; });

            this.networkService.searchMembers(apiFilter).subscribe({
                next: (result) => {
                    this.currentPage = result.page;
                    this.totalItems.set(result.totalItems);
                    this.hasMore.set(result.hasNextPage);
                    // Append new items
                    this.members.update(prev => [...prev, ...result.items]);
                    resolve(true);
                },
                error: () => {
                    this.hasMore.set(false);
                    resolve(false);
                }
            });
        });
    }

    /** Re-observe sentinel after DOM update */
    private reobserve() {
        if (this.observer && this.sentinelRef?.nativeElement) {
            this.observer.unobserve(this.sentinelRef.nativeElement);
            this.observer.observe(this.sentinelRef.nativeElement);
        }
    }

    viewProfile(id: number) {
        this.networkService.getMemberProfile(id).subscribe({
            next: (p) => this.selectedMember.set(p),
            error: () => this.notify.error('Failed to load profile details')
        });
    }

    sendMessage(id: number) {
        this.router.navigate(['/portal/messages'], { queryParams: { thread: id } });
        this.selectedMember.set(null);
    }
}
