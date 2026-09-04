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
});
