import { Component, inject, signal, OnInit, OnDestroy, Input, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { getECPositionName, getCurrentECPosition, PROFESSIONAL_SECTORS, getBloodGroupName, MEMBERSHIP_TYPE_OPTIONS, LOOKUP_GROUPS } from '../../core/constants/app.constants';
import { LookupService, LookupOption } from '../../core/services/lookup.service';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
    selector: 'app-directory',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, ImgFallbackDirective],
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
    private lookupService = inject(LookupService);
    orgConfig = inject(OrgConfigService);

    // 58.2: table is the default view for the full page; the compact embedded mode (used inside
    // e.g. the messages "start conversation" picker) always stays card-based regardless of this.
    viewMode = signal<'table' | 'card'>('table');

    members = signal<MemberSummary[]>([]);
    loading = signal(true);      // initial/search load
    loadingMore = signal(false); // scroll-triggered load
    hasMore = signal(true);
    totalItems = signal(0);

    // 82.42: sourced from /lookups/{group} via LookupService, filled in ngOnInit.
    years: number[] = [];
    memberCategories: LookupOption[] = [];
    sectors = PROFESSIONAL_SECTORS;
    membershipTypes = MEMBERSHIP_TYPE_OPTIONS; // 35.3: template drove this off a hardcoded list

    selectedMember = signal<any | null>(null);

    getMajorDisplay(degree: string | undefined, subject: string | undefined): string {
        if (!degree) return '';
        const major = subject || 'None';
        return major && major !== 'None' ? `in ${major}` : '';
    }

    // 'Unknown' is a real stored value for professionalSector (see PROFESSIONAL_SECTORS), so it
    // has to be treated as absent here or the directory renders "at Unknown" to the public.
    getProfessionDisplay(designation?: string, sector?: string): string {
        const clean = (v?: string) => {
            const t = (v ?? '').trim();
            return !t || t.toLowerCase() === 'unknown' || t.toLowerCase() === 'n/a' ? '' : t;
        };
        const role = clean(designation);
        const org = clean(sector);
        if (role && org) return `${role} at ${org}`;
        return role || org;
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
        bloodGroup: '',
        category: '' as any,
        membershipType: ''
    };

    ngOnInit() {
        this.doSearch(); // Initial load
        this.lookupService.getAcademicYears().subscribe(years => this.years = years);
        this.lookupService.getOptions(LOOKUP_GROUPS.MemberCategory).subscribe(opts => this.memberCategories = opts);
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
                category: this.filters.category || undefined,
                membershipType: this.filters.membershipType || undefined,
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
