import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LedgerService } from './ledger.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('LedgerService', () => {
    let service: LedgerService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [LedgerService]
        });
        service = TestBed.inject(LedgerService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should fetch financial records', () => {
        const dummyRecords = [{ id: 1, amount: 100, description: 'Test' }];
        service.getRecords({ year: 2024 }).subscribe(records => {
            expect(records.length).toBe(1);
            expect(records).toEqual(dummyRecords as any);
        });

        const req = httpMock.expectOne(r => r.url === API_ENDPOINTS.LEDGER && r.params.get('year') === '2024');
        expect(req.request.method).toBe('GET');
        req.flush(dummyRecords);
    });

    it('should fetch ledger summary', () => {
        const dummySummary = { year: 2024, totalIncome: 1000, totalExpense: 500, balance: 500 };
        service.getSummary(2024).subscribe(summary => {
            expect(summary).toEqual(dummySummary);
        });

        const req = httpMock.expectOne(r => r.url === `${API_ENDPOINTS.LEDGER}/summary` && r.params.get('year') === '2024');
        expect(req.request.method).toBe('GET');
        req.flush(dummySummary);
    });

    it('should add a new record', () => {
        const newRecord = { amount: 200, description: 'New' };
        service.addRecord(newRecord).subscribe(record => {
            expect(record.amount).toBe(200);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.LEDGER);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(newRecord);
        req.flush({ id: 2, ...newRecord });
    });
});
