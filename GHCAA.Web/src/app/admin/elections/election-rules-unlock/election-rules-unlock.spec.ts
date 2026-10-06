import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { ElectionRulesUnlock } from './election-rules-unlock';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ElectionRulesUnlockDto } from '../../../core/models/election.models';

// TODO 37.13z.
describe('ElectionRulesUnlock', () => {
    let component: ElectionRulesUnlock;
    let electionsMock: any;
    let notifyMock: any;
    let confirmMock: any;
    const unlock: ElectionRulesUnlockDto = {
        id: 1, openedByUserId: 1, openedBy: 'shalin', reason: 'Wrong ballot order after freeze',
        openedAt: '2026-10-05T10:00:00Z', expiresAt: '2026-10-05T10:30:00Z'
    };

    beforeEach(async () => {
        electionsMock = {
            getRulesUnlock: vi.fn().mockReturnValue(of(null)),
            openRulesUnlock: vi.fn().mockReturnValue(of(unlock)),
            closeRulesUnlock: vi.fn().mockReturnValue(of(undefined))
        };
        notifyMock = createNotificationServiceMock();
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };
        await TestBed.configureTestingModule({
            imports: [ElectionRulesUnlock],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: notifyMock },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();
        component = TestBed.createComponent(ElectionRulesUnlock).componentInstance;
    });

    it('does not call the API until asked, since every call needs step-up', () => {
        expect(electionsMock.getRulesUnlock).not.toHaveBeenCalled();
        component.load();
        expect(component.loaded()).toBe(true);
        expect(component.unlock()).toBeNull();
    });

    it('rounds minutes left up and never below zero', () => {
        const opened = new Date(unlock.openedAt).getTime();
        expect(component.minutesLeft(unlock, opened)).toBe(30);
        expect(component.minutesLeft(unlock, opened + 29 * 60000 + 1000)).toBe(1);
        expect(component.minutesLeft(unlock, opened + 31 * 60000)).toBe(0);
    });

    it('checks the reason length and the minutes like the API does', () => {
        component.reason = 'too short';
        expect(component.validate()).toContain('at least 20');
        component.reason = 'x'.repeat(1001);
        expect(component.validate()).toContain('at most 1000');
        component.reason = 'Wrong ballot order after freeze';
        component.minutes = 61;
        expect(component.validate()).toContain('1 to 60');
        component.minutes = 0;
        expect(component.validate()).toContain('1 to 60');
        component.minutes = 15;
        expect(component.validate()).toBeNull();
    });

    it('opens after the confirm dialog and resets the form', async () => {
        component.reason = '  Wrong ballot order after freeze  ';
        component.minutes = 15;
        await component.open();
        expect(electionsMock.openRulesUnlock).toHaveBeenCalledWith('Wrong ballot order after freeze', 15);
        expect(component.unlock()).toEqual(unlock);
        expect(component.reason).toBe('');
        expect(component.minutes).toBe(30);
    });

    it('does not open when the reason is too short or the dialog is cancelled', async () => {
        component.reason = 'short';
        await component.open();
        expect(notifyMock.warning).toHaveBeenCalled();

        component.reason = 'Wrong ballot order after freeze';
        confirmMock.confirm.mockReturnValue(of(false));
        await component.open();
        expect(electionsMock.openRulesUnlock).not.toHaveBeenCalled();
    });

    it('closes, and reloads when the server says nothing is open', () => {
        component.unlock.set(unlock);
        component.close();
        expect(component.unlock()).toBeNull();

        electionsMock.closeRulesUnlock.mockReturnValue(throwError(() => new Error('409')));
        component.close();
        expect(electionsMock.getRulesUnlock).toHaveBeenCalled();
        expect(component.busy()).toBe(false);
    });
});
