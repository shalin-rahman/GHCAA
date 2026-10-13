import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Ledger } from './ledger';
import { LedgerService } from '../../core/services/ledger.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('Ledger Component', () => {
    let component: Ledger;
    let fixture: ComponentFixture<Ledger>;
    let ledgerServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        ledgerServiceMock = {
            getSummary: vi.fn().mockReturnValue(of({ totalIncome: 0, totalExpense: 0, netBalance: 0 })),
            getRecords: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0 })),
            addRecord: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [Ledger],
            providers: [
                { provide: LedgerService, useValue: ledgerServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Ledger);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load summary and records on init', () => {
        expect(ledgerServiceMock.getSummary).toHaveBeenCalled();
        expect(ledgerServiceMock.getRecords).toHaveBeenCalled();
    });

    it('offers only income categories for an income record', () => {
        component.newRecord.recordType = 'Income';
        const values = component.categoryOptions().map(o => o.value);
        expect(values).toContain('MembershipFee');
        expect(values).not.toContain('Salary');
    });

    it('offers only expense categories for an expense record', () => {
        component.newRecord.recordType = 'Expense';
        const values = component.categoryOptions().map(o => o.value);
        expect(values).toContain('Salary');
        expect(values).not.toContain('MembershipFee');
    });

    it('resets the category when the type changes and the old one no longer applies', () => {
        component.newRecord.financialCategory = 'MembershipFee';
        component.newRecord.recordType = 'Expense';
        component.onTypeChange();
        expect(component.newRecord.financialCategory).toBe('Event');
    });

    it('keeps the category when it is valid for the new type', () => {
        component.newRecord.financialCategory = 'Other';
        component.newRecord.recordType = 'Expense';
        component.onTypeChange();
        expect(component.newRecord.financialCategory).toBe('Other');
    });
});
