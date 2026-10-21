// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ElectionDocumentVerify } from './election-document-verify';
import { ElectionsService } from '../../../core/services/elections.service';
import { ElectionDocumentVerifyDto } from '../../../core/models/election.models';

describe('ElectionDocumentVerify', () => {
    const record: ElectionDocumentVerifyDto = {
        reference: 'E-000012', electionId: 12, electionTitle: 'EC 2026', phase: 'Declared',
        formCode: 'ER-27C', formTitle: 'Declaration of result'
    };
    let elections: any;

    function create(params: Record<string, string>, call: any = of(record)) {
        elections = { verifyDocument: vi.fn().mockReturnValue(call) };
        TestBed.configureTestingModule({
            imports: [ElectionDocumentVerify],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(params) } } }
            ]
        });
        const fixture = TestBed.createComponent(ElectionDocumentVerify);
        fixture.detectChanges();
        return { el: fixture.nativeElement as HTMLElement };
    }

    it('FR-25: shows the election and form the QR code points at', () => {
        const { el } = create({ id: '12', code: 'ER-27C' });

        expect(elections.verifyDocument).toHaveBeenCalledWith(12, 'ER-27C');
        expect(el.textContent).toContain('Election form recognised');
        expect(el.textContent).toContain('ER-27C Declaration of result');
        expect(el.textContent).toContain('EC 2026 (E-000012)');
        expect(el.textContent).toContain('Declared');
    });

    it('FR-25: says no form matches on a 404', () => {
        const { el } = create({ id: '12', code: 'ER-99' }, throwError(() => new HttpErrorResponse({ status: 404 })));

        expect(el.textContent).toContain('No election form matches this reference.');
        expect(el.textContent).not.toContain('Election form recognised');
    });

    it('rejects a malformed link without calling the API', () => {
        const { el } = create({ id: 'abc', code: 'ER-10' });

        expect(elections.verifyDocument).not.toHaveBeenCalled();
        expect(el.textContent).toContain('not a valid election form reference');
    });
});
