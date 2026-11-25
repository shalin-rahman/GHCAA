import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { ElectionPrintRecords } from './election-print-records';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { AdminElectionDto, ElectionFormPrintRowDto } from '../../../core/models/election.models';

// TODO 37.16i.
describe('ElectionPrintRecords', () => {
    let electionsMock: any;
    let confirmMock: any;
    const row = (over: Partial<ElectionFormPrintRowDto> = {}): ElectionFormPrintRowDto => ({
        id: 'a1', formCode: 'ER-10', printedAt: '2026-11-20T09:00:00Z', printedByRole: 'Returning Officer',
        phaseAtPrint: 'Counting', revokedAt: null, revokeReason: null, ...over
    });
    const election = (permissions: string[]) => ({ id: 7, title: 'EC 2026', phase: 'Counting', myPermissions: permissions } as unknown as AdminElectionDto);

    const create = async (value: AdminElectionDto, rows: ElectionFormPrintRowDto[] = [row()]) => {
        electionsMock = {
            getPrintRecords: vi.fn().mockReturnValue(of(rows)),
            revokePrint: vi.fn().mockReturnValue(of(undefined))
        };
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };
        await TestBed.configureTestingModule({
            imports: [ElectionPrintRecords],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: createNotificationServiceMock() },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(ElectionPrintRecords);
        fixture.componentRef.setInput('election', value);
        fixture.detectChanges();
        return fixture;
    };
    const query = (fixture: any, id: string): HTMLButtonElement | null => fixture.nativeElement.querySelector(`[data-testid="${id}"]`);

    it('lists the rows in a table', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        expect(electionsMock.getPrintRecords).toHaveBeenCalledWith(7);
        expect(fixture.nativeElement.querySelector('.table-wrap .data-table tbody td').textContent).toContain('ER-10');
    });

    // A colspan cell in tbody makes the global .data-table rule hide the column headers.
    it('keeps the column headers while the revoke form is open', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        query(fixture, 'revoke-open')!.click();
        fixture.detectChanges();
        await fixture.whenStable();
        expect(fixture.nativeElement.querySelector('textarea')).not.toBeNull();
        expect(fixture.nativeElement.querySelector('.data-table tbody td[colspan]')).toBeNull();
    });

    it('reason under 3 characters keeps Revoke disabled', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        query(fixture, 'revoke-open')!.click();
        fixture.detectChanges();
        await fixture.whenStable();
        const type = async (value: string) => {
            const box: HTMLTextAreaElement = fixture.nativeElement.querySelector('textarea');
            box.value = value;
            box.dispatchEvent(new Event('input'));
            fixture.detectChanges();
            await fixture.whenStable();
        };

        await type('  a b  ');
        expect(query(fixture, 'revoke-submit')!.disabled).toBe(true);
        await fixture.componentInstance.revoke(row());
        expect(electionsMock.revokePrint).not.toHaveBeenCalled();

        await type('abc');
        expect(query(fixture, 'revoke-submit')!.disabled).toBe(false);
    });

    it('confirms, then revokes with the trimmed reason and marks the row', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        const component = fixture.componentInstance;
        component.reason = '  Misprint  ';
        await component.revoke(row());

        expect(confirmMock.confirm).toHaveBeenCalled();
        expect(electionsMock.revokePrint).toHaveBeenCalledWith(7, 'a1', 'Misprint');
        expect(component.rows()[0].revokeReason).toBe('Misprint');
        expect(component.rows()[0].revokedAt).not.toBeNull();
    });

    it('does not revoke when the confirm is cancelled', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        confirmMock.confirm.mockReturnValue(of(false));
        fixture.componentInstance.reason = 'Misprint';
        await fixture.componentInstance.revoke(row());
        expect(electionsMock.revokePrint).not.toHaveBeenCalled();
    });

    it('hides Revoke without both permissions', async () => {
        for (const permissions of [['Count'], ['ManageSetup'], []]) {
            TestBed.resetTestingModule();
            const fixture = await create(election(permissions));
            expect(query(fixture, 'revoke-open')).toBeNull();
        }
    });

    it('hides Revoke on a revoked row', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']), [row({ revokedAt: '2026-11-21T10:00:00Z', revokeReason: 'Misprint' })]);
        expect(query(fixture, 'revoke-open')).toBeNull();
        expect(fixture.nativeElement.textContent).toContain('Misprint');
    });

    it('shows a load error, not the empty message, when the list fails', async () => {
        await TestBed.configureTestingModule({
            imports: [ElectionPrintRecords],
            providers: [
                { provide: ElectionsService, useValue: { getPrintRecords: vi.fn().mockReturnValue(throwError(() => new Error('500'))) } },
                { provide: NotificationService, useValue: createNotificationServiceMock() },
                { provide: ConfirmDialogService, useValue: { confirm: vi.fn() } }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(ElectionPrintRecords);
        fixture.componentRef.setInput('election', election(['Count', 'ManageSetup']));
        fixture.detectChanges();
        expect(fixture.nativeElement.textContent).toContain('Could not load the print records.');
        expect(fixture.nativeElement.textContent).not.toContain('No filled forms have been printed yet.');
    });

    it('opens one confirm dialog on a double click', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']));
        const component = fixture.componentInstance;
        component.reason = 'Misprint';
        await Promise.all([component.revoke(row()), component.revoke(row())]);
        expect(confirmMock.confirm).toHaveBeenCalledTimes(1);
        expect(electionsMock.revokePrint).toHaveBeenCalledTimes(1);
    });

    it('shows the empty message when nothing was printed', async () => {
        const fixture = await create(election(['Count', 'ManageSetup']), []);
        expect(fixture.nativeElement.textContent).toContain('No filled forms have been printed yet.');
    });
});
