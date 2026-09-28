import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminElections } from './admin-elections';
import { ElectionsService } from '../../core/services/elections.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AdminElectionDto } from '../../core/models/election.models';
import { ExportUtil } from '../../core/utils/export.util';

describe('AdminElections ballot key', () => {
    let component: AdminElections;
    let electionsMock: any;
    let notifyMock: any;
    let confirmMock: any;
    const election = { id: 7, title: 'EC 2026', phase: 'Campaign', ballotKeyFingerprint: null, positions: [], candidates: [] } as unknown as AdminElectionDto;

    beforeEach(async () => {
        electionsMock = {
            getAdminElections: vi.fn().mockReturnValue(of([election])),
            setBallotKey: vi.fn().mockReturnValue(of({ ...election, ballotKeyFingerprint: 'ab12' })),
            count: vi.fn().mockReturnValue(of([]))
        };
        notifyMock = createNotificationServiceMock();
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };

        await TestBed.configureTestingModule({
            imports: [AdminElections],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: notifyMock },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();

        component = TestBed.createComponent(AdminElections).componentInstance;
    });

    afterEach(() => vi.restoreAllMocks());

    // jsdom's File has no text(), which browsers do have.
    const keyFile = (text: string) => ({ text: () => Promise.resolve(text) }) as unknown as File;

    // FR-39: the key can be set up to the campaign and not once voting has opened.
    it('offers the key only before polling', () => {
        expect(component.canSetBallotKey(election)).toBe(true);
        expect(component.canSetBallotKey({ ...election, phase: 'Polling' })).toBe(false);
        expect(component.canSetBallotKey({ ...election, phase: 'Counting' })).toBe(false);
    });

    it('saves the private key before the public key is sent', async () => {
        const order: string[] = [];
        vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => { order.push('save'); });
        electionsMock.setBallotKey.mockImplementation(() => { order.push('upload'); return of({ ...election, ballotKeyFingerprint: 'ab12' }); });

        await component.createBallotKey(election);

        expect(order).toEqual(['save', 'upload']);
        expect(electionsMock.setBallotKey).toHaveBeenCalledWith(7, expect.stringMatching(/^[A-Za-z0-9+/=]{300,}$/));
        expect(component.elections()[0].ballotKeyFingerprint).toBe('ab12');
    }, 30000);

    it('does nothing when the officer cancels', async () => {
        confirmMock.confirm.mockReturnValue(of(false));
        const save = vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => {});

        await component.createBallotKey(election);

        expect(save).not.toHaveBeenCalled();
        expect(electionsMock.setBallotKey).not.toHaveBeenCalled();
    });

    it('sends the key file text to the count', async () => {
        const file = keyFile('-----BEGIN PRIVATE KEY-----\nAB\nCD\n-----END PRIVATE KEY-----\n');
        const input = { files: [file], value: 'key.pem' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(electionsMock.count).toHaveBeenCalledWith(7, 'ABCD');
        expect(input.value).toBe('');
        expect(notifyMock.success).toHaveBeenCalled();
    });

    it('leaves a failed count message to the HTTP interceptor', async () => {
        electionsMock.count.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 400 })));
        const input = { files: [keyFile('ABCD')], value: '' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(notifyMock.error).not.toHaveBeenCalled();
        expect(component.countingId()).toBeNull();
    });
});
