// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormPrintVerify } from './form-print-verify';
import { ElectionsService } from '../../../core/services/elections.service';
import { OrgConfigService } from '../../../core/services/org-config.service';
import { ElectionFormPrintVerifyDto } from '../../../core/models/election.models';

describe('FormPrintVerify', () => {
    const printId = '3f2b8c1e-5d4a-4e7b-9c2d-1a2b3c4d5e6f';
    const hash = 'a'.repeat(64);
    const record: ElectionFormPrintVerifyDto = {
        id: printId, reference: 'E-000012', electionTitle: 'EC 2026', formCode: 'ER-10', formTitle: 'Nomination Paper',
        printedAt: '2026-11-10T09:30:00', printedByRole: 'Chief Commissioner', phaseAtPrint: 'Nomination',
        contentHash: hash, revoked: false, revokedAt: null
    };
    let elections: any;

    function create(params: Record<string, string>, call: any = of(record)) {
        elections = { verifyPrint: vi.fn().mockReturnValue(call) };
        TestBed.configureTestingModule({
            imports: [FormPrintVerify],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: OrgConfigService, useValue: { dateFormat: signal('dd-MM-yyyy') } },
                { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(params) } } }
            ]
        });
        const fixture = TestBed.createComponent(FormPrintVerify);
        fixture.detectChanges();
        return { el: fixture.nativeElement as HTMLElement };
    }

    it('FR-25: shows election, form, print time, printer role and hash', () => {
        const { el } = create({ id: printId });

        expect(elections.verifyPrint).toHaveBeenCalledWith(printId);
        expect(el.textContent).toContain('Print record found');
        expect(el.textContent).toContain('ER-10 Nomination Paper');
        expect(el.textContent).toContain('EC 2026 (E-000012)');
        expect(el.textContent).toContain('10-11-2026');
        expect(el.textContent).toContain('by the Chief Commissioner');
        expect(el.textContent).toContain(hash);
    });

    it('FR-25: marks a revoked print', () => {
        const { el } = create({ id: printId }, of({ ...record, revoked: true, revokedAt: '2026-11-11T08:00:00' }));

        expect(el.textContent).toContain('Print revoked');
        expect(el.textContent).toContain('withdrew this print on 11-11-2026');
        expect(el.textContent).not.toContain('Print record found');
    });

    it('FR-25: says no record matches on a 404', () => {
        const { el } = create({ id: printId }, throwError(() => new HttpErrorResponse({ status: 404 })));

        expect(el.textContent).toContain('No print record matches this reference.');
        expect(el.textContent).not.toContain('Print record found');
    });

    it('rejects a malformed id without calling the API', () => {
        const { el } = create({ id: 'not-a-guid' });

        expect(elections.verifyPrint).not.toHaveBeenCalled();
        expect(el.textContent).toContain('not a valid print reference');
    });
});
