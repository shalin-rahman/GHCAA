import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { EcPositionsComponent } from './ec-positions';
import { EcPositionService } from '../../../core/services/ec-position.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

describe('EcPositionsComponent', () => {
    let component: EcPositionsComponent;
    let fixture: ComponentFixture<EcPositionsComponent>;
    let positionServiceMock: any;
    let notify: any;
    let confirmMock: any;
    const president = { id: 1, key: 'President', name: 'President', nameBn: null, displayOrder: 1, seatCount: 1, isActive: true, showOnBoard: true, signsAsHead: true, signsAsSecretary: false };
    const patron = { id: 16, key: 'Patron', name: 'Patron', nameBn: null, displayOrder: 16, seatCount: 2, isActive: false, showOnBoard: true, signsAsHead: false, signsAsSecretary: false };

    beforeEach(async () => {
        positionServiceMock = {
            getAll: vi.fn().mockReturnValue(of([patron, president])),
            create: vi.fn().mockReturnValue(of({})),
            update: vi.fn().mockReturnValue(of({})),
            retire: vi.fn().mockReturnValue(of({})),
            restore: vi.fn().mockReturnValue(of({})),
            remove: vi.fn().mockReturnValue(of({})),
            getInvalidRows: vi.fn().mockReturnValue(of([])),
            repair: vi.fn().mockReturnValue(of({}))
        };
        notify = createNotificationServiceMock();
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };

        await TestBed.configureTestingModule({
            imports: [EcPositionsComponent],
            providers: [
                { provide: EcPositionService, useValue: positionServiceMock },
                { provide: NotificationService, useValue: notify },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(EcPositionsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('FR-34: lists every position, retired ones included, in display order', () => {
        expect(component.positions().map(p => p.id)).toEqual([1, 16]);
        expect(fixture.nativeElement.textContent).toContain('Retired');
    });

    it('FR-34: a new position starts after the last display order and is sent trimmed', async () => {
        component.openNew();
        expect(component.form().displayOrder).toBe(17);

        component.patch({ name: '  Adviser  ', seatCount: 3 });
        await component.save();

        expect(positionServiceMock.create).toHaveBeenCalledWith(expect.objectContaining({ name: 'Adviser', nameBn: null, seatCount: 3 }));
        expect(component.editingId()).toBeNull();
    });

    it('FR-34: editing sends an update for that id', async () => {
        component.openEdit(president);
        component.patch({ name: 'Chair' });
        await component.save();

        expect(positionServiceMock.update).toHaveBeenCalledWith(1, expect.objectContaining({ name: 'Chair' }));
    });

    it('FR-34: a blank name or a seat count outside 1 to 20 is not sent', async () => {
        component.openNew();
        await component.save();
        component.patch({ name: 'Adviser', seatCount: 21 });
        await component.save();

        expect(component.formError()).toContain('Seats');
        expect(positionServiceMock.create).not.toHaveBeenCalled();
    });

    it('FR-34: the API reason is shown when a delete is refused', async () => {
        positionServiceMock.remove.mockReturnValue(throwError(() => ({ error: { detail: 'President has been held. Retire it instead.' } })));

        await component.remove(president);

        expect(confirmMock.confirm).toHaveBeenCalledWith(expect.objectContaining({ danger: true }));
        expect(notify.error).toHaveBeenCalledWith('President has been held. Retire it instead.');
    });

    it('FR-34: nothing is deleted when the admin cancels the confirm', async () => {
        confirmMock.confirm.mockReturnValue(of(false));

        await component.remove(patron);

        expect(positionServiceMock.remove).not.toHaveBeenCalled();
    });

    it('FR-34: retire and restore reload the list', async () => {
        await component.retire(president);
        await component.restore(patron);

        expect(positionServiceMock.retire).toHaveBeenCalledWith(1);
        expect(positionServiceMock.restore).toHaveBeenCalledWith(16);
        expect(positionServiceMock.getAll).toHaveBeenCalledTimes(3);
    });

    it('FR-34: rows holding a missing position are listed and repaired with the picked position', async () => {
        const row = { kind: 'election-seat', id: 7, position: 0, description: 'Election 2024' };
        positionServiceMock.getInvalidRows.mockReturnValue(of([row]));
        component.load();
        fixture.detectChanges();
        expect(fixture.nativeElement.textContent).toContain('Election 2024');

        await component.repair(row);
        expect(positionServiceMock.repair).not.toHaveBeenCalled();

        component.pickRepair(row, 1);
        await component.repair(row);
        expect(positionServiceMock.repair).toHaveBeenCalledWith(row, 1);
    });

    it('FR-34: a repair refused by the API shows its reason', async () => {
        const row = { kind: 'committee-term', id: 3, position: 0, description: 'A in 2024' };
        positionServiceMock.repair.mockReturnValue(throwError(() => ({ error: { detail: 'Every President seat in that period is held. End one term first.' } })));
        component.pickRepair(row, 1);
        await component.repair(row);
        expect(notify.error).toHaveBeenCalledWith('Every President seat in that period is held. End one term first.');
    });

    it('FR-34: display order outside 0 to 999 blocks the save', () => {
        component.openNew();
        component.patch({ name: 'Adviser', displayOrder: 1000 });
        expect(component.formError()).toContain('Display order');
        component.patch({ displayOrder: -1 });
        expect(component.formError()).toContain('Display order');
    });

    it('FR-34: Escape leaves the form first, then closes the dialog', () => {
        const closed = vi.fn();
        component.closed.subscribe(closed);
        component.openNew();
        component.onEscape();
        expect(component.editingId()).toBeNull();
        expect(closed).not.toHaveBeenCalled();
        component.onEscape();
        expect(closed).toHaveBeenCalled();
    });
});
