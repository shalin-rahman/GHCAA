import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_ENDPOINTS, LOOKUP_GROUPS } from '../constants/app.constants';

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
    [LOOKUP_GROUPS.MemberCategory]: [
        { value: 'None', label: 'None' },
        { value: 'LifelongPatron', label: 'Lifelong Patron' },
        { value: 'Sponsor', label: 'Sponsor' },
        { value: 'Advisor', label: 'Advisor' },
        { value: 'Mentor', label: 'Mentor' },
        { value: 'Recruiter', label: 'Recruiter' },
        { value: 'Active', label: 'Active Member' },
        { value: 'Volunteer', label: 'Volunteer' },
        { value: 'Contributor', label: 'Contributor' },
        { value: 'Guest', label: 'Guest' },
        { value: 'Student', label: 'Student' }
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
    ]
};

function academicYearsFallback(): number[] {
    const currentYear = new Date().getFullYear();
    const startYear = 1950;
    return Array.from({ length: currentYear - startYear + 1 }, (_, i) => currentYear - i);
}

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);

    getLookups(lookupGroup?: string, silent: boolean = false): Observable<any[]> {
        const url = lookupGroup ? `${API_ENDPOINTS.LOOKUPS}/${lookupGroup}` : API_ENDPOINTS.LOOKUPS;
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<any[]>(url, { headers });
    }

    /** A lookup group as {value,label} options, falling back to LOOKUP_FALLBACKS if the group has no active rows. */
    getOptions(group: string): Observable<LookupOption[]> {
        return this.getLookups(group, true).pipe(
            map(items => (items && items.length)
                ? items.map(i => ({ value: String(i.value), label: String(i.label) }))
                : (LOOKUP_FALLBACKS[group] || []))
        );
    }

    /** Passing-year list for academic history dropdowns, sourced from the PassingYear group. */
    getAcademicYears(): Observable<number[]> {
        return this.getLookups(LOOKUP_GROUPS.PassingYear, true).pipe(
            map(items => (items && items.length)
                ? items.map(i => Number(i.value)).filter(n => !isNaN(n))
                : academicYearsFallback())
        );
    }

    getStats(silent: boolean = false): Observable<any> {
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<any>(`${API_ENDPOINTS.LOOKUPS}/stats`, { headers });
    }
}
