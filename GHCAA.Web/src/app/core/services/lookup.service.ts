import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of, throwError } from 'rxjs';
import { catchError, map, shareReplay } from 'rxjs/operators';
import {
    API_ENDPOINTS, HSC_SUBJECT_GROUPS, LOOKUP_GROUPS,
    getMemberCategoryOptions, getMembershipTypeOptions, PASSING_YEAR_FALLBACK, TSHIRT_SIZES
} from '../constants/app.constants';
import { getSilentHeaders } from '../utils/http.util';

export interface LookupOption {
    value: string;
    label: string;
}

// 82.42: fallback text for lookup groups the /lookups endpoint doesn't have seeded rows for yet.
// Same pattern GHCAA.Mobile's DropdownService already uses — ask the API first, only fall back
// here if it comes back empty — so this is the one place the fallback wording lives instead of
// every screen keeping its own copy. Wording matches DropdownService's fallback text exactly,
// which is what closed the "Pending" vs "Pending Approval" / "Inactive" vs "Inactive (Unpaid)"
// drift between the two clients.
const LOOKUP_FALLBACKS: Record<string, LookupOption[]> = {
    [LOOKUP_GROUPS.MembershipStatus]: [
        { value: 'Applied', label: 'Pending Approval' },
        { value: 'Active', label: 'Active Member' },
        { value: 'InactivePayment', label: 'Inactive (Unpaid)' },
        { value: 'InactiveResigned', label: 'Inactive (Resigned)' },
        { value: 'Terminated', label: 'Terminated' }
    ],
    [LOOKUP_GROUPS.Gender]: [
        { value: 'None', label: 'Not Specified' },
        { value: 'Male', label: 'Male' },
        { value: 'Female', label: 'Female' },
        { value: 'Other', label: 'Other' }
    ],
    [LOOKUP_GROUPS.BloodGroup]: [
        { value: 'Unknown', label: 'Not Specified' },
        { value: 'APositive', label: 'A+' },
        { value: 'ANegative', label: 'A-' },
        { value: 'BPositive', label: 'B+' },
        { value: 'BNegative', label: 'B-' },
        { value: 'OPositive', label: 'O+' },
        { value: 'ONegative', label: 'O-' },
        { value: 'ABPositive', label: 'AB+' },
        { value: 'ABNegative', label: 'AB-' }
    ],
    [LOOKUP_GROUPS.JobCategory]: [
        { value: 'IT', label: 'IT & Software Development' },
        { value: 'Finance', label: 'Finance & Banking' },
        { value: 'Engineering', label: 'Engineering & Construction' },
        { value: 'Marketing', label: 'Marketing & Sales' },
        { value: 'Education', label: 'Education & Research' },
        { value: 'Health', label: 'Healthcare & Pharma' },
        { value: 'PublicSector', label: 'Govt. & Public Sector' },
        { value: 'Mentorship', label: 'Mentorship & Career Guidance' },
        { value: 'Other', label: 'Other Opportunities' }
    ],
    [LOOKUP_GROUPS.TShirtSize]: TSHIRT_SIZES,
    [LOOKUP_GROUPS.DiscussionCategory]: [{ value: 'Other', label: 'Other' }]
};

// These two read the OrgConfig labels at the time of the call, so they are never stale.
function fallbackFor(group: string): LookupOption[] {
    if (group === LOOKUP_GROUPS.MemberCategory) return getMemberCategoryOptions();
    if (group === LOOKUP_GROUPS.MembershipType) return getMembershipTypeOptions();
    return LOOKUP_FALLBACKS[group] || [];
}

export interface AcademicData {
    certificates: string[];
    subjects: string[];
    sectors: string[];
}

// The served AcademicSubject group has no HSC streams, so they go after 'None'. 'None' is added
// even when the server omits it, because saved records use it for "no subject".
function withHscGroups(subjects: string[]): string[] {
    const rest = subjects.filter(s => s !== 'None' && !HSC_SUBJECT_GROUPS.includes(s));
    return ['None', ...HSC_SUBJECT_GROUPS, ...rest];
}

function academicYearsFallback(): number[] {
    const { EARLIEST, FUTURE_YEARS, UTC_OFFSET_HOURS } = PASSING_YEAR_FALLBACK;
    const currentYear = new Date(Date.now() + UTC_OFFSET_HOURS * 60 * 60 * 1000).getUTCFullYear();
    const latestYear = currentYear + FUTURE_YEARS;
    return Array.from({ length: latestYear - EARLIEST + 1 }, (_, i) => latestYear - i);
}

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);
    // One request per group per session. The web client has no lookup editor, so nothing here
    // needs to clear it. A failed request is dropped so the next caller tries again.
    private groupCache = new Map<string, Observable<any[]>>();

    getLookups(lookupGroup?: string, silent: boolean = false): Observable<any[]> {
        const url = lookupGroup ? `${API_ENDPOINTS.LOOKUPS}/${lookupGroup}` : API_ENDPOINTS.LOOKUPS;
        const headers = getSilentHeaders(silent);
        return this.http.get<any[]>(url, { headers });
    }

    private cachedGroup(group: string): Observable<any[]> {
        let cached = this.groupCache.get(group);
        if (!cached) {
            cached = this.getLookups(group, true).pipe(
                catchError(err => {
                    this.groupCache.delete(group);
                    return throwError(() => err);
                }),
                shareReplay(1)
            );
            this.groupCache.set(group, cached);
        }
        return cached;
    }

    /** A lookup group as {value,label} options, falling back to fallbackFor if the group has no active rows. */
    getOptions(group: string): Observable<LookupOption[]> {
        return this.cachedGroup(group).pipe(
            map(items => (items && items.length)
                ? items.map(i => ({ value: String(i.value), label: String(i.label) }))
                : fallbackFor(group))
        );
    }

    /** getOptions that also falls back when the request fails. */
    getOptionsOrFallback(group: string): Observable<LookupOption[]> {
        return this.getOptions(group).pipe(catchError(() => of(fallbackFor(group))));
    }

    getValues(group: string): Observable<string[]> {
        return this.getOptionsOrFallback(group).pipe(map(options => options.map(o => o.value)));
    }

    getAcademicData(): Observable<AcademicData> {
        return forkJoin({
            certificates: this.getValues(LOOKUP_GROUPS.Degree),
            subjects: this.getValues(LOOKUP_GROUPS.AcademicSubject).pipe(map(withHscGroups)),
            sectors: this.getValues(LOOKUP_GROUPS.ProfessionalSector)
        });
    }

    /** Passing-year list for academic history dropdowns, sourced from the PassingYear group. */
    getAcademicYears(): Observable<number[]> {
        return this.cachedGroup(LOOKUP_GROUPS.PassingYear).pipe(
            map(items => (items && items.length)
                ? items.map(i => Number(i.value)).filter(n => !isNaN(n))
                : academicYearsFallback())
        );
    }

    getStats(silent: boolean = false): Observable<any> {
        const headers = getSilentHeaders(silent);
        return this.http.get<any>(`${API_ENDPOINTS.LOOKUPS}/stats`, { headers });
    }
}
