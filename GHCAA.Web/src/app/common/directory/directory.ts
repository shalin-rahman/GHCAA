import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { getPositionLabel, getCurrentECLabel, PROFESSIONAL_SECTORS, getBloodGroupName, LOOKUP_GROUPS, SEARCH_DEBOUNCE_MS, ADMIN_ROLE, SUPER_ADMIN_ROLE, ROUTES } from '../../core/constants/app.constants';
import { AuthService } from '../../core/services/auth.service';
import { debounce } from '../../core/utils/debounce.util';
import { LookupService, LookupOption } from '../../core/services/lookup.service';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { LazyImgDirective } from '../directives/lazy-img.directive';
import { OrgConfigService } from '../../core/services/org-config.service';
import { LoadMoreComponent } from '../load-more/load-more';
import { PageHeaderComponent } from '../page-header/page-header.component';
import { ModalHeaderComponent } from '../modal-header/modal-header.component';

@Component({
    selector: 'app-directory',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, ImgFallbackDirective, LazyImgDirective, PageHeaderComponent, ModalHeaderComponent, LoadMoreComponent],
    templateUrl: './directory.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    styleUrl: './directory.scss'
})
export class Directory implements OnInit, OnDestroy {
    @Input() isCompact: boolean = false;
    @Input() disableProfile: boolean = false;

    getPositionLabel = getPositionLabel;
    getCurrentECLabel = getCurrentECLabel;
    getBloodGroupName = getBloodGroupName;
    private networkService = inject(NetworkingService);
    private notify = inject(NotificationService);
    private router = inject(Router);
    private lookupService = inject(LookupService);
    orgConfig = inject(OrgConfigService);
    private auth = inject(AuthService);

    // The API returns name and batch only to anonymous visitors, so the page hides
    // the filters and fields it has no data for.
    get publicListing(): boolean {
        return this.disableProfile && !this.auth.isAuthenticated();
    }

    canAdminEdit = computed(() => this.auth.hasRole(ADMIN_ROLE) || this.auth.hasRole(SUPER_ADMIN_ROLE));

    // 58.2: table is the default view for the full page; the compact embedded mode (used inside
    // e.g. the messages "start conversation" picker) always stays card-based regardless of this.
    viewMode = signal<'table' | 'card'>('table');

    members = signal<MemberSummary[]>([]);
    loading = signal(true);      // initial/search load
    loadingMore = signal(false); // scroll-triggered load
    hasMore = signal(true);
    totalItems = signal(0);

    // 82.42: sourced from /lookups/{group} via LookupService, filled in ngOnInit.
    years = signal<number[]>([]);
    memberCategories = signal<LookupOption[]>([]);
    sectors = PROFESSIONAL_SECTORS;
    membershipTypes = signal<LookupOption[]>([]); // 62.33: sourced from LOOKUP_GROUPS.MembershipType, filled in ngOnInit

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
    // Bumped by each new search so a page still loading for the old filters is dropped.
    private searchVersion = 0;
    private readonly PAGE_SIZE = 20;
    private debouncedSearch = debounce(() => this.doSearch(), SEARCH_DEBOUNCE_MS);

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
        this.lookupService.getAcademicYears().subscribe(years => this.years.set(years));
        this.lookupService.getOptions(LOOKUP_GROUPS.MemberCategory).subscribe(opts => this.memberCategories.set(opts));
        this.lookupService.getOptions(LOOKUP_GROUPS.MembershipType).subscribe(opts => this.membershipTypes.set(opts));
    }

    ngOnDestroy() {
        this.debouncedSearch.cancel();
    }

    /** Called when filters change — resets to page 1 */
    search() {
        this.debouncedSearch();
    }

    private doSearch() {
        const version = ++this.searchVersion;
        this.currentPage = 1;
        this.members.set([]);
        this.hasMore.set(true);
        this.loadingMore.set(false);
        this.loading.set(true);
        this.fetchPage(1).then(() => {
            if (version === this.searchVersion) this.loading.set(false);
        });
    }

    loadNextPage() {
        if (!this.hasMore() || this.loadingMore()) return;
        const version = this.searchVersion;
        this.loadingMore.set(true);
        const nextPage = this.currentPage + 1;
        this.fetchPage(nextPage).then(() => {
            if (version === this.searchVersion) this.loadingMore.set(false);
        });
    }

    private fetchPage(page: number): Promise<boolean> {
        const version = this.searchVersion;
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
                    if (version !== this.searchVersion) return resolve(false);
                    this.currentPage = result.page;
                    this.totalItems.set(result.totalItems);
                    this.hasMore.set(result.hasNextPage ?? (result.page < result.totalPages));
                    // Append new items
                    this.members.update(prev => [...prev, ...result.items]);
                    resolve(true);
                },
                error: () => {
                    if (version !== this.searchVersion) return resolve(false);
                    this.hasMore.set(false);
                    resolve(false);
                }
            });
        });
    }

    viewProfile(id: number) {
        this.networkService.getMemberProfile(id).subscribe({
            next: (p) => this.selectedMember.set(p),
            error: () => this.notify.error('Failed to load profile details')
        });
    }

    editAsAdmin(id: number) {
        this.router.navigate([ROUTES.ADMIN_MEMBERS], { queryParams: { edit: id } });
    }

    sendMessage(id: number) {
        this.router.navigate(['/portal/messages'], { queryParams: { thread: id } });
        this.selectedMember.set(null);
    }
}
